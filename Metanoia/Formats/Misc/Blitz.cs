using System;
using Metanoia.Modeling;
using System.IO;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Formats.Misc
{
    [FormatAttribute(Extension = ".gcp", Description = "BlitzXboxModel")]
    public class Blitz : IModelFormat
    {

        private List<OBE> OBEs = new List<OBE>();


        public void Open(FileItem File)
        {
            using (DataReader reader = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
            {
                reader.BigEndian = true;
                var TimeStamp = reader.ReadInt32();

                var Align = reader.ReadUInt32();

                reader.ReadInt32();

                var FCount = reader.ReadInt32();
                var InfoOffset = reader.ReadUInt32() * Align;

                reader.BaseStream.Position = 0x28;
                var NameOffset = reader.ReadUInt32() * Align;
                
                reader.Position = InfoOffset;

                for (var f = 0; f < FCount; f++)
                {
                    var offset = reader.ReadUInt32() * Align;
                    var crc = reader.ReadUInt32();
                    var size = reader.ReadUInt32() * Align;
                    var name_off = reader.ReadUInt32() + NameOffset;
                    var is_file = reader.ReadUInt32();

                    // optional
                    var zero = reader.ReadUInt32();
                    var crc1 = reader.ReadUInt32();
                    var crc2 = reader.ReadUInt32();

                    if (is_file == 0)
                        continue;

                    var temp = reader.Position;

                    reader.Position = name_off;
                    var name = reader.ReadString();
                    Console.WriteLine(name + " " + offset.ToString("x") + " - " + size.ToString("x"));

                    reader.Position = offset;
                    byte[] data = reader.ReadBytes((int)size);
                    //Directory.CreateDirectory("out\\");
                    //File.WriteAllBytes(@"out\" + name, data);

                    if (name.EndsWith("obe"))
                    {
                        var obe = new OBE();
                        obe.ParseOBE(data);
                        OBEs.Add(obe);
                    }
                    //var filedata = this.data.slice(offset, offset + size);
                    //dds = loadImage(filedata, true);

                    reader.Position = temp;

                    //break;
                }
                
            }
        }

        

        public GenericModel ToGenericModel()
        {
            return OBEs[0].GetGenericModel();
        }
    }
}
