using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Metanoia.Tools
{
    public class Decompress
    {
        public static bool CheckLevel5Zlib(byte[] input, out byte[] t)
        {
            var b = input;
            t = input;
            if (b.Length >= 6)
            {
                var decomLength = (b[0] & 0xFF) | ((b[1] & 0xFF) << 8) | ((b[2] & 0xFF) << 16) | ((b[3] & 0xFF) << 24);
                if (b[4] == 0x78)
                {
                    t = new byte[b.Length - 4];
                    Array.Copy(b, 4, t, 0, b.Length - 4);
                    t = ZLIB(t);
                    Console.WriteLine("ZLIB: " + decomLength.ToString("X") + " " + t.Length.ToString("X"));
                    return true;
                }
            }
            return false;
        }

        public static byte[] Level5Decom(byte[] b)
        {
            int tableType = (b[0] & 0xFF);

            byte[] t;

            if (CheckLevel5Zlib(b, out t))
                return t;

            switch (tableType & 0xF)
            {
                case 0x01:
                    t = (lzss_Decompress(b));
                    break;
                case 0x02:
                    t = Huffman_Decompress(b, 4);
                    break;
                case 0x03:
                    t = Huffman_Decompress(b, 8);
                    break;
                case 0x04:
                    t = (rle_Decompress(b));
                    break;
                default:
                    t = new byte[b.Length - 4];
                    Array.Copy(b, 4, t, 0, b.Length - 4);
                    break;
            }

            return t;
        }

        public static byte[] YAY0(byte[] fileData)
        {
            DataReader code = new DataReader(new MemoryStream(fileData));
            DataReader count = new DataReader(new MemoryStream(fileData));
            DataReader data = new DataReader(new MemoryStream(fileData));
            code.BigEndian = true;
            count.BigEndian = true;
            data.BigEndian = true;

            code.Seek(0);
            Console.WriteLine(code.ReadString(4));
            code.Seek(4);
            int decompressedSize = code.ReadInt32();
            count.Seek(code.ReadUInt32());
            data.Seek(code.ReadUInt32());

            byte[] outputArray = new byte[decompressedSize];

            int outPosition = 0;
            uint validBitsCount = 0;
            byte currentCodeByte = 0;

            while (outPosition < decompressedSize)
            {
                if (validBitsCount <= 0)
                {
                    currentCodeByte = (byte)code.ReadByte();
                    validBitsCount = 8;
                }
                if ((currentCodeByte & 0x80) == 0x80)
                {
                    outputArray[outPosition++] = (byte)data.ReadByte();
                }
                else
                {
                    int c = count.ReadInt16();

                    int distance = (c & 0xFFF);

                    int startOffset = (outPosition - (distance + 1));

                    int byteCount = ((c >> 12) & 0xF);

                    if (byteCount == 0)
                    {
                        byteCount = (data.ReadByte() + 0x10);
                    }

                    // Take into consideration the two bytes for the count by adding two to the byte count.
                    byteCount += 2;

                    // Copy the run data.
                    for (int j = 0; j < byteCount; j++) outputArray[outPosition++] = outputArray[startOffset++];
                }
                currentCodeByte <<= 1;
                validBitsCount--;
            }

            code.Dispose();
            data.Dispose();
            count.Dispose();

            return outputArray;
        }

        public static byte[] YAZ0(byte[] data)
        {
            DataReader f = new DataReader(new MemoryStream(data));

            f.BigEndian = true;
            f.Seek(4);
            int uncompressedSize = f.ReadInt32();
            f.Seek(0x10);

            byte[] src = f.ReadBytes(data.Length - 0x10);
            byte[] dst = new byte[uncompressedSize];

            int srcPlace = 0, dstPlace = 0; //current read/write positions

            uint validBitCount = 0; //number of valid bits left in "code" byte
            byte currCodeByte = 0;
            while (dstPlace < uncompressedSize)
            {
                //read new "code" byte if the current one is used up
                if (validBitCount == 0)
                {
                    currCodeByte = src[srcPlace];
                    ++srcPlace;
                    validBitCount = 8;
                }

                if ((currCodeByte & 0x80) != 0)
                {
                    //straight copy
                    dst[dstPlace] = src[srcPlace];
                    dstPlace++;
                    srcPlace++;
                }
                else
                {
                    //RLE part
                    byte byte1 = src[srcPlace];
                    byte byte2 = src[srcPlace + 1];
                    srcPlace += 2;

                    uint dist = (uint)(((byte1 & 0xF) << 8) | byte2);
                    uint copySource = (uint)(dstPlace - (dist + 1));

                    uint numBytes = (uint)(byte1 >> 4);
                    if (numBytes == 0)
                    {
                        numBytes = (uint)(src[srcPlace] + 0x12);
                        srcPlace++;
                    }
                    else
                        numBytes += 2;

                    //copy run
                    for (int i = 0; i < numBytes; ++i)
                    {
                        dst[dstPlace] = dst[copySource];
                        copySource++;
                        dstPlace++;
                    }
                }

                //use next bit from "code" byte
                currCodeByte <<= 1;
                validBitCount -= 1;
            }

            f.Dispose();

            return dst;
        }

        public static byte[] LZ11(byte[] instream, bool header = true)
        {
            int pointer = 0;

            if (header)
            {
                byte type = (byte)(instream[pointer++] & 0xFF);
                if (type != 0x11)
                    throw new Exception("The provided stream is not a valid LZ-0x11 "
                            + "compressed stream (invalid type 0x)");
                int decompressedSize = ((instream[pointer++] & 0xFF))
                        | ((instream[pointer++] & 0xFF) << 8) | ((instream[pointer++] & 0xFF) << 16);

                if (decompressedSize == 0)
                {
                    decompressedSize = ((instream[pointer++] & 0xFF))
                            | ((instream[pointer++] & 0xFF) << 8) | ((instream[pointer++] & 0xFF) << 16)
                            | ((instream[pointer++] & 0xFF) << 24);
                    ;
                }
            }

            List<byte> outstream = new List<byte>();//[decompressedSize];
            int outpointer = 0;

            // the maximum 'DISP-1' is still 0xFFF.
            int bufferLength = 0x1000;
            int[] buffer = new int[bufferLength];
            int bufferOffset = 0;

            int currentOutSize = 0;
            int flags = 0, mask = 1;

            while (pointer < instream.Length)
            {
                // (throws when requested new flags byte is not available)
                // region Update the mask. If all flag bits have been read, get a
                // new set.
                // the current mask is the mask used in the previous run. So if it
                // masks the
                // last flag bit, get a new flags byte.
                if (mask == 1)
                {
                    //				if (ReadBytes >= inLength)
                    //					throw new Exception("Not enough data");
                    flags = (instream[pointer++] & 0xFF);
                    //				if (flags < 0)
                    //					throw new Exception("Stream too short");
                    mask = 0x80;
                }
                else
                {
                    mask >>= 1;
                }

                // bit = 1 <=> compressed.
                if ((flags & mask) > 0)
                {
                    // (throws when not enough bytes are available)
                    // region Get length and displacement('disp') values from next
                    // 2, 3 or 4 bytes

                    // read the first byte first, which also signals the size of the
                    // compressed block
                    //				if (ReadBytes >= inLength)
                    //					throw new Exception("Not enough data2");
                    int byte1 = (instream[pointer++] & 0xFF);
                    //				if (byte1 < 0)
                    //					throw new Exception("Stream too short2");

                    int length = byte1 >> 4;
                    int disp = -1;

                    if (length == 0)
                    {
                        // region case 0; 0(B C)(D EF) + (0x11)(0x1) = (LEN)(DISP)

                        // case 0:
                        // data = AB CD EF (with A=0)
                        // LEN = ABC + 0x11 == BC + 0x11
                        // DISP = DEF + 1

                        // we need two more bytes available
                        //					if (ReadBytes + 1 >= inLength)
                        //						throw new Exception("Not enough data3");
                        int byte2 = (instream[pointer++] & 0xFF);

                        int byte3 = (instream[pointer++] & 0xFF);

                        //					if (byte3 < 0)
                        //						throw new Exception("Stream too short3");

                        length = (((byte1 & 0x0F) << 4) | (byte2 >> 4)) + 0x11;
                        disp = (((byte2 & 0x0F) << 8) | byte3) + 0x1;

                        // endregion
                    }
                    else if (length == 1)
                    {
                        // region case 1: 1(B CD E)(F GH) + (0x111)(0x1) =
                        // (LEN)(DISP)

                        // case 1:
                        // data = AB CD EF GH (with A=1)
                        // LEN = BCDE + 0x111
                        // DISP = FGH + 1

                        // we need three more bytes available
                        //					if (ReadBytes + 2 >= inLength)
                        //						throw new Exception("Not enough data3");
                        int byte2 = (instream[pointer++] & 0xFF);

                        int byte3 = (instream[pointer++] & 0xFF);

                        int byte4 = (instream[pointer++] & 0xFF);

                        //					if (byte4 < 0)
                        //						throw new Exception("Stream too short3");

                        length = (((byte1 & 0x0F) << 12) | (byte2 << 4) | (byte3 >> 4)) + 0x111;
                        disp = (((byte3 & 0x0F) << 8) | byte4) + 0x1;

                        // endregion
                    }
                    else
                    {
                        // region case > 1: (A)(B CD) + (0x1)(0x1) = (LEN)(DISP)

                        // case other:
                        // data = AB CD
                        // LEN = A + 1
                        // DISP = BCD + 1

                        // we need only one more byte available
                        //					if (ReadBytes >= inLength)
                        //						throw new Exception("Not enough data3");
                        int byte2 = (instream[pointer++] & 0xFF);

                        //					if (byte2 < 0)
                        //						throw new Exception("Stream too short3");

                        length = ((byte1 & 0xF0) >> 4) + 0x1;
                        disp = (((byte1 & 0x0F) << 8) | byte2) + 0x1;

                        // endregion
                    }

                    // endregion

                    int bufIdx = bufferOffset + bufferLength - disp;
                    for (int i = 0; i < length; i++)
                    {
                        int next = buffer[bufIdx % bufferLength];
                        bufIdx++;
                        outstream.Add((byte)(next & 0xFF));
                        // outstream.WriteByte(next);
                        buffer[bufferOffset] = next;
                        bufferOffset = (bufferOffset + 1) % bufferLength;
                    }
                    currentOutSize += length;
                }
                else
                {
                    // if (ReadBytes >= inLength)
                    // throw new NotEnoughDataException(currentOutSize,
                    // decompressedSize);
                    int next = (instream[pointer++] & 0xFF);
                    // if (next < 0)
                    // throw new StreamTooShortException();

                    outstream.Add((byte)(next & 0xFF));
                    currentOutSize++;
                    buffer[bufferOffset] = next;
                    bufferOffset = (bufferOffset + 1) % bufferLength;
                }

            }
            return outstream.ToArray();

        }

        public static byte[] lzss_Decompress(byte[] data)
        {
            List<byte> o = new List<byte>();

            int p = 4;
            int op = 0;

            int mask = 0;
            int flag = 0;

            while (p < data.Length)
            {
                if (mask == 0)
                {
                    flag = (data[p++] & 0xFF);
                    //					System.out.println(Integer.toHexString(flag));
                    mask = 0x80;
                }

                if ((flag & mask) == 0)
                {
                    if (p + 1 > data.Length) break;
                    o.Add(data[p++]);
                    op++;
                }
                else
                {
                    if (p + 2 > data.Length) break;
                    int dat = ((data[p++] & 0xFF) << 8) | (data[p++] & 0xFF);
                    int pos = (dat & 0x0FFF) + 1;
                    int length = (dat >> 12) + 3;

                    //				System.out.println(Integer.toHexString(dat) + "\t" + pos + "\t" + length);

                    for (int i = 0; i < length; i++)
                    {
                        if (op - pos >= 0)
                        {
                            o.Add(o[op - pos >= o.Count ? 0 : op - pos]);
                            op++;
                        }
                    }
                }
                mask >>= 1;
            }


            //Console.WriteLine("decompress " + p.ToString("x") + " " + data.Length.ToString("x"));
            return o.ToArray();
        }

        public static byte[] rle_Decompress(byte[] instream)
        {
            using (var input = new MemoryStream(instream))
            using (var output = new MemoryStream())
            {
                var decoder = new RleDecoder();
                decoder.Decode(input, output);

                return output.ToArray();
            }
        }

        // RLE From Kuriimu2

        public class RleHeaderlessDecoder
        {
            public void Decode(Stream input, Stream output, int decompressedSize)
            {
                while (output.Length < decompressedSize)
                {
                    var flag = input.ReadByte();
                    if ((flag & 0x80) > 0)
                    {
                        var repetitions = (flag & 0x7F) + 3;
                        output.Write(Enumerable.Repeat((byte)input.ReadByte(), repetitions).ToArray(), 0, repetitions);
                    }
                    else
                    {
                        var length = flag + 1;
                        var uncompressedData = new byte[length];
                        input.Read(uncompressedData, 0, length);
                        output.Write(uncompressedData, 0, length);
                    }
                }
            }
        }

        public class RleDecoder
        {
            private readonly RleHeaderlessDecoder _decoder;

            public RleDecoder()
            {
                _decoder = new RleHeaderlessDecoder();
            }

            public void Decode(Stream input, Stream output)
            {
                var compressionHeader = new byte[4];
                input.Read(compressionHeader, 0, 4);
                if ((compressionHeader[0] & 0x7) != 0x4)
                    throw new Exception("Level5 Rle");

                var decompressedSize = (compressionHeader[0] >> 3) | (compressionHeader[1] << 5) |
                                       (compressionHeader[2] << 13) | (compressionHeader[3] << 21);

                _decoder.Decode(input, output, decompressedSize);
            }

            public void Dispose()
            {
                // Nothing to dispose
            }
        }

            // HuffmanDecoder & HuffmanHeaderlessDecoder From Kuriimu2
            public class HuffmanDecoder
        {
            private readonly int _bitDepth;
            private readonly HuffmanHeaderlessDecoder _decoder;

            public HuffmanDecoder(int bitDepth, NibbleOrder nibbleOrder)
            {
                _bitDepth = bitDepth;

                _decoder = new HuffmanHeaderlessDecoder(bitDepth, nibbleOrder);
            }

            public void Decode(Stream input, Stream output)
            {
                var compressionHeader = new byte[4];
                input.Read(compressionHeader, 0, 4);

                var huffmanMode = _bitDepth == 4 ? 2 : 3;
                if ((compressionHeader[0] & 0x7) != huffmanMode)
                    throw new InvalidDataException($"Level5 Huffman{_bitDepth}");

                var decompressedSize = (compressionHeader[0] >> 3) | (compressionHeader[1] << 5) |
                                       (compressionHeader[2] << 13) | (compressionHeader[3] << 21);

                _decoder.Decode(input, output, decompressedSize);
            }

            public void Dispose()
            {
                // nothing to dispose
            }
        }

        public class HuffmanHeaderlessDecoder
        {
            private readonly int _bitDepth;
            private readonly NibbleOrder _nibbleOrder;

            public HuffmanHeaderlessDecoder(int bitDepth, NibbleOrder nibbleOrder)
            {
                _bitDepth = bitDepth;
                _nibbleOrder = nibbleOrder;
            }

            public void Decode(Stream input, Stream output, int decompressedSize)
            {
                var result = new byte[decompressedSize * 8 / _bitDepth];

                using (var br = new BinaryReader(input, Encoding.ASCII, true))
                {
                    var treeSize = br.ReadByte();
                    var treeRoot = br.ReadByte();
                    var treeBuffer = br.ReadBytes(treeSize * 2);

                    for (int i = 0, code = 0, next = 0, pos = treeRoot, resultPos = 0; resultPos < result.Length; i++)
                    {
                        if (i % 32 == 0)
                            code = br.ReadInt32();

                        next += ((pos & 0x3F) << 1) + 2;
                        var direction = (code >> (31 - i)) % 2 == 0 ? 2 : 1;
                        var leaf = (pos >> 5 >> direction) % 2 != 0;

                        pos = treeBuffer[next - direction];
                        if (leaf)
                        {
                            result[resultPos++] = (byte)pos;
                            pos = treeRoot;
                            next = 0;
                        }
                    }
                }

                if (_bitDepth == 8)
                    output.Write(result, 0, result.Length);
                else
                {
                    var combinedData = _nibbleOrder == NibbleOrder.LowNibbleFirst ?
                        Enumerable.Range(0, decompressedSize).Select(j => (byte)(result[2 * j] | (result[2 * j + 1] << 4))).ToArray() :
                        Enumerable.Range(0, decompressedSize).Select(j => (byte)((result[2 * j] << 4) | result[2 * j + 1])).ToArray();

                    output.Write(combinedData, 0, combinedData.Length);
                }
            }
        }

        public enum NibbleOrder
        {
            LowNibbleFirst,
            HighNibbleFirst
        }

        public static byte[] Huffman_Decompress(byte[] b, byte atype)
        {
            using (var input = new MemoryStream(b))
            using (var output = new MemoryStream())
            {
                var decoder = new HuffmanDecoder(atype, NibbleOrder.LowNibbleFirst);
                decoder.Decode(input, output);

                return output.ToArray();
            }
        }

        public static byte[] ZLIB(byte[] data)
        {
            var stream = new MemoryStream();
            var ms = new MemoryStream(data);
            ms.ReadByte();
            ms.ReadByte();
            var zlibStream = new DeflateStream(ms, CompressionMode.Decompress);
            byte[] buffer = new byte[2048];
            while (true)
            {
                int size = zlibStream.Read(buffer, 0, buffer.Length);
                if (size > 0)
                    stream.Write(buffer, 0, buffer.Length);
                else
                    break;
            }
            zlibStream.Close();
            return stream.ToArray();
        }


        private static int fbuf = 0;
        public static byte[] PRS_Mod(byte[] compData, int decompSize, int compSize)
        {
            fbuf = 0;
            return PRS_8ing(decompSize, compData, compSize);
        }

        private static int prs_8ing_get_bits(int n, byte[] sbuf, ref int sptr, ref int blen)
        {
            int retv;

            retv = 0;
            while (n != 0)
            {
                retv <<= 1;
                if (blen == 0)
                {
                    fbuf = sbuf[sptr];
                    //if(*sptr<256)
                    //{ fprintf(stderr, "[%02x] ", fbuf&0xff); fflush(0); }
                    sptr++;
                    blen = 8;
                }

                if ((fbuf & 0x80) != 0)
                {
                    retv |= 1;
                }

                fbuf <<= 1;
                blen--;
                n--;
            }

            return retv;
        }

        private static byte[] PRS_8ing(int dlen, byte[] sbuf, int slen)
        {
            byte[] dbuf = new byte[dlen];
            int sptr;
            int dptr;
            int i;
            int flag;
            int len;
            int pos;

            int blen = 0;

            sptr = 0;
            dptr = 0;
            while (sptr < slen)
            {
                flag = prs_8ing_get_bits(1, sbuf, ref sptr, ref blen);
                if (flag == 1)
                {
                    //if(sptr<256)
                    //{ fprintf(stderr, "%02x ", (u8)sbuf[sptr]); fflush(0); }
                    if (dptr < dlen)
                    {
                        dbuf[dptr++] = sbuf[sptr++];
                    }
                }
                else
                {
                    flag = prs_8ing_get_bits(1, sbuf, ref sptr, ref blen);
                    if (flag == 0)
                    {
                        len = prs_8ing_get_bits(2, sbuf, ref sptr, ref blen) + 2;
                        pos = (int)(sbuf[sptr++] | 0xffffff00);
                    }
                    else
                    {
                        pos = (int)((sbuf[sptr++] << 8) | 0xffff0000);
                        pos |= sbuf[sptr++] & 0xff;
                        len = pos & 0x07;
                        pos >>= 3;
                        if (len == 0)
                        {
                            len = (sbuf[sptr++] & 0xff) + 1;
                        }
                        else
                        {
                            len += 2;
                        }
                    }
                    //if(sptr<256)
                    //{ fprintf(stderr, "<%08x(%08x): %08x %d> \n", dptr, dlen, pos, len); fflush(0); }
                    pos += dptr;
                    for (i = 0; i < len; i++)
                    {
                        if (dptr < dlen)
                        {
                            dbuf[dptr++] = dbuf[(uint)pos++];
                        }
                    }
                }
            }

            return dbuf;
        }



        public static byte[] SRD_Decomp(byte[] data)
        {
            List<byte> o = new List<byte>();

            using (DataReader r = new DataReader(data))
            {
                r.BigEndian = true;

                if (r.ReadString(4) != "$CMP")
                    throw new InvalidDataException();

                r.Seek(0x10);
                var decompSize = r.ReadInt32();
                var compSize = r.ReadInt32();
                r.Skip(0x10);

                while (true)
                {
                    var cmp_mode = r.ReadString(4);
                    if (!cmp_mode.StartsWith("$CL") && !cmp_mode.Equals("$CR0"))
                        break;

                    var chunk_dec_size = r.ReadInt32();
                    var chunk_cmp_size = r.ReadInt32();
                    r.ReadInt32();

                    var chunk = r.ReadBytes(chunk_cmp_size - 0x10);

                    if (!cmp_mode.Equals("$CR0"))
                        chunk = SRC_DEC_CHUNK(chunk, cmp_mode);

                    o.AddRange(chunk);
                }
            }

            return o.ToArray();
        }

        public static byte[] SRC_DEC_CHUNK(byte[] chunk, string cmp_mode)
        {
            List<byte> o = new List<byte>();

            using (DataReader r = new DataReader(chunk))
            {
                r.BigEndian = true;


            }

            return o.ToArray();
        }

    }
}