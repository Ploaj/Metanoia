using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;

namespace Metanoia.Formats.PS3
{
    [FormatAttribute(Extension = ".mol", Description = "Little Big Planet Model")]
    public class LBP_MOL : IModelFormat
    {
        public void Open(byte[] Data)
        {
            byte[] DecompressedData = null;
            using (DataReader reader = new DataReader(new MemoryStream(Data)))
            {
                reader.BigEndian = true;
                reader.Seek(0x14); // skip header stuff
                ushort CompressedChunkCount = reader.ReadUInt16();

                ushort[] CompressedSize = new ushort[CompressedChunkCount];
                for(int i = 0; i < CompressedChunkCount; i++)
                {
                    CompressedSize[i] = reader.ReadUInt16();
                    reader.ReadUInt16();
                }

                MemoryStream data = new MemoryStream();
                using (BinaryWriter o = new BinaryWriter(data))
                {
                    foreach (ushort compsize in CompressedSize)
                    {
                        o.Write(Metanoia.Tools.Decompress.ZLIB(reader.ReadBytes(compsize)));
                    }
                    DecompressedData = data.GetBuffer();
                    data.Close();
                    //File.WriteAllBytes("Decomp.bin", DecompressedData);
                }

                reader.PrintPosition();
            }

            using (DataReader reader = new DataReader(new MemoryStream(DecompressedData)))
            {
                // bit pack hell
                reader.Skip(11);

                byte ExpressionStringCount = reader.ReadByte();
                string[] ExpressionStrings = new string[ExpressionStringCount];
                for (int i = 0; i < ExpressionStringCount; i++)
                {
                    ExpressionStrings[i] = reader.ReadString(0x10);
                    Console.WriteLine(ExpressionStrings[i]);
                }


                reader.Seek(0x20C);
                byte Command = reader.ReadByte();
                bool Go = true;
                while (Go)
                {
                    switch (Command)
                    {
                        case 0x02:
                            Console.WriteLine(reader.ReadSingle() + " " + reader.ReadSingle());
                            break;
                        case 0x43:
                            reader.Skip(4);
                            break;
                        default:
                            Console.WriteLine($"Unknown Command {Command}");
                            Go = false;
                            break;
                    }
                    Command = reader.ReadByte();
                }
            }
        }

        public GenericModel ToGenericModel()
        {
            return null;
        }
    }
    
}
