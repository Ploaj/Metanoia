using System;
using System.IO;

namespace Metanoia
{
    /// <summary>
    /// Wrapper for binary reader that is endianness independent
    /// </summary>
    public class DataReader : BinaryReader
    {
        public bool BigEndian { get; set; } = false;

        public DataReader(Stream input) : base(input)
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
            char ch;
            while ((ch = ReadChar()) != 0)
                str = str + ch;
            return str;
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

        public uint Position()
        {
            return (uint)BaseStream.Position;
        }

        public void WriteInt32At(int Value, int Position)
        {
            byte[] data = Reverse(BitConverter.GetBytes(Value));
            long temp = BaseStream.Position;
            BaseStream.Position = Position;
            BaseStream.Write(data, 0, data.Length);
            BaseStream.Position = temp;
        }

        public byte[] GetStreamData()
        {
            long temp = Position();
            Seek(0);
            byte[] data = ReadBytes((int)BaseStream.Length);
            Seek((uint)temp);
            return data;
        }

        public byte[] GetSection(uint Offset, int Size)
        {
            long temp = Position();
            Seek(Offset);
            byte[] data = ReadBytes(Size);
            Seek((uint)temp);
            return data;
        }
    }
}
