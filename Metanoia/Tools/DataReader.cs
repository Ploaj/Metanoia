﻿using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Metanoia
{
    /// <summary>
    /// Wrapper for binary reader that is endianness independent
    /// </summary>
    public class DataReader : BinaryReader
    {
        public bool BigEndian { get; set; } = false;

        public long Length { get => BaseStream.Length; }

        public DataReader(byte[] data) : base(new MemoryStream(data))
        {

        }

        public DataReader(byte[] data, Encoding encoding) : base(new MemoryStream(data), encoding)
        {

        }

        public DataReader(Stream input) : base(input)
        {
        }

        public DataReader(string path) : base(new FileStream(path, FileMode.Open))
        {
        }

        public DataReader(FileItem item) : base(item.EmbeddedFile ? (Stream)new MemoryStream(item.GetFileBinary()) : new FileStream(item.FilePath, FileMode.Open))
        {
        }

        public override Int16 ReadInt16()
        {
            return BitConverter.ToInt16(Reverse(base.ReadBytes(2)), 0);
        }

        public override UInt16 ReadUInt16()
        {
            return BitConverter.ToUInt16(Reverse(base.ReadBytes(2)), 0);
        }

        public override Int32 ReadInt32()
        {
            return BitConverter.ToInt32(Reverse(base.ReadBytes(4)), 0);
        }

        public override UInt32 ReadUInt32()
        {
            return BitConverter.ToUInt32(Reverse(base.ReadBytes(4)), 0);
        }

        public override Int64 ReadInt64()
        {
            return BitConverter.ToInt64(Reverse(base.ReadBytes(8)), 0);
        }

        public override UInt64 ReadUInt64()
        {
            return BitConverter.ToUInt64(Reverse(base.ReadBytes(8)), 0);
        }

        public override float ReadSingle()
        {
            return BitConverter.ToSingle(Reverse(base.ReadBytes(4)), 0);
        }

        public void Skip(uint Size)
        {
            BaseStream.Seek(Size, SeekOrigin.Current);
        }

        public void Seek(uint Position)
        {
            BaseStream.Seek(Position, SeekOrigin.Begin);
        }

        public byte[] Reverse(byte[] b)
        {
            if (BitConverter.IsLittleEndian && BigEndian)
                Array.Reverse(b);
            return b;
        }

        public void PrintPosition()
        {
            Console.WriteLine("Stream at 0x{0}", BaseStream.Position.ToString("X"));
        }

        public override string ReadString()
        {
            string str = "";
            byte ch;
            while ((ch = ReadByte()) != 0)
                str = str + (char)ch;
            return str;
        }

        public string ReadString(Encoding encoding)
        {
            List<byte> text = new List<byte>();

            byte ch;
            while ((ch = ReadByte()) != 0)
            {
                text.Add(ch);
            }

            Console.WriteLine(BitConverter.ToString(text.ToArray()).Replace("-",""));

            return encoding.GetString(text.ToArray());
        }

        public string ReadString(int Size)
        {
            string str = "";
            for(int i = 0; i < Size; i++)
            {
                byte b = ReadByte();
                if(b != 0)
                {
                    str = str + (char)b;
                }
            }
            return str;
        }

        public float ReadHalfSingle()
        {
            int hbits = ReadInt16();

            int mant = hbits & 0x03ff;            // 10 bits mantissa
            int exp = hbits & 0x7c00;            // 5 bits exponent
            if (exp == 0x7c00)                   // NaN/Inf
                exp = 0x3fc00;                    // -> NaN/Inf
            else if (exp != 0)                   // normalized value
            {
                exp += 0x1c000;                   // exp - 15 + 127
                if (mant == 0 && exp > 0x1c400)  // smooth transition
                    return BitConverter.ToSingle(BitConverter.GetBytes((hbits & 0x8000) << 16
                        | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)                  // && exp==0 -> subnormal
            {
                exp = 0x1c400;                    // make it normal
                do
                {
                    mant <<= 1;                   // mantissa * 2
                    exp -= 0x400;                 // decrease exp by 1
                } while ((mant & 0x400) == 0); // while not normal
                mant &= 0x3ff;                    // discard subnormal bit
            }                                     // else +/-0 -> +/-0
            return BitConverter.ToSingle(BitConverter.GetBytes(          // combine all parts
                (hbits & 0x8000) << 16          // sign  << ( 31 - 15 )
                | (exp | mant) << 13), 0);         // value << ( 23 - 10 )
        }

        public uint Position
        {
            get { return (uint)BaseStream.Position; }
            set
            {
                BaseStream.Position = value;
            }
        }

        public void WriteInt32At(int Value, int Position)
        {
            byte[] data = Reverse(BitConverter.GetBytes(Value));
            long temp = BaseStream.Position;
            BaseStream.Position = Position;
            BaseStream.Write(data, 0, data.Length);
            BaseStream.Position = temp;
        }

        public void Align(int alignment)
        {
            if(Position % alignment != 0)
            {
                Position += (uint)(alignment-(Position % alignment));
            }
        }

        public byte[] GetStreamData()
        {
            long temp = Position;
            Seek(0);
            byte[] data = ReadBytes((int)BaseStream.Length);
            Seek((uint)temp);
            return data;
        }

        public byte[] GetSection(uint Offset, int Size)
        {
            long temp = Position;
            Seek(Offset);
            byte[] data = ReadBytes(Size);
            Seek((uint)temp);
            return data;
        }

        internal string ReadString(uint offset, int size)
        {
            string str = "";

            var temp = Position;
            Position = offset;

            if(size == -1)
            {
                byte b = ReadByte();
                while(b != 0)
                {
                    str = str + (char)b;
                    b = ReadByte();
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    byte b = ReadByte();
                    if (b != 0)
                    {
                        str = str + (char)b;
                    }
                }
            }

            Position = temp;

            return str;
        }

        public T[] ReadStructArray<T>(int Size)
        {
            T[] arr = new T[Size];
            for (int i = 0; i < Size; i++)
            {
                arr[i] = ReadStruct<T>();
            }
            return arr;
        }

        public T ReadStruct<T>()
        {
            byte[] bytes = ReadBytes(Marshal.SizeOf(typeof(T)));

            MaybeAdjustEndianness(typeof(T), bytes);

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
        
        private int bitPosition = 0;

        private byte Peek()
        {
            byte b = ReadByte();
            BaseStream.Position -= 1;
            return b;
        }

        public int ReadBits(int bitCount)
        {
            byte b = Peek();
            int value = 0;
            int LE = 0;
            int bitIndex = 0;
            for (int i = 0; i < bitCount; i++)
            {
                byte bit = (byte)((b & (0x1 << (bitPosition))) >> (bitPosition));
                value |= (bit << (LE + bitIndex));
                bitPosition++;
                bitIndex++;
                if (bitPosition >= 8)
                {
                    bitPosition = 0;
                    b = ReadByte();
                    b = Peek();
                }
                if (bitIndex >= 8)
                {
                    bitIndex = 0;
                    if (LE + 8 > bitCount)
                    {
                        LE = bitCount - 1;
                    }
                    else
                        LE += 8;
                }
            }

            return value;
        }
        private void MaybeAdjustEndianness(Type type, byte[] data, int startOffset = 0)
        {
            if (!BigEndian)
            {
                // nothing to change => return
                return;
            }

            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;

                if (fieldType == typeof(string))
                    // don't swap bytes for strings
                    continue;

                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                // handle enums
                if (fieldType.IsEnum)
                    fieldType = Enum.GetUnderlyingType(fieldType);

                // check for sub-fields to recurse if necessary
                var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

                var effectiveOffset = startOffset + offset;

                if (subFields.Length == 0)
                {
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                }
                else
                {
                    // recurse
                    MaybeAdjustEndianness(fieldType, data, effectiveOffset);
                }
            }
        }
        
    }
}
