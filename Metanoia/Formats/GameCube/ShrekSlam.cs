using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using OpenTK;
using Metanoia.Tools;

namespace Metanoia.Formats.GameCube
{
    //Incomplete
    //[FormatAttribute(Extension = ".gcmesh", Description = "Shrek Slam")]
    public class ShrekSlam : IModelFormat
    {
        private GenericModel Model = new GenericModel();

        public void Open(FileItem File)
        {
            Model = new GenericModel();

            using (DataReader r = new DataReader(new System.IO.FileStream(File.FilePath, System.IO.FileMode.Open)))
            {
                r.BigEndian = true;

                r.Seek(4);
                int unknownCount = r.ReadInt32();
                int sectionCount = r.ReadInt32();
                var sectionOffset = r.ReadUInt32();

                r.Seek(sectionOffset);
                for(int i = 0; i < sectionCount; i++)
                {
                    var flags1 = r.ReadInt16();
                    var flags2 = r.ReadInt16();
                    var dataOffset = r.ReadUInt32();
                    var nameOffset = r.ReadUInt32();

                    var temp = r.Position;
                    r.Seek(nameOffset);
                    var nameHash = r.ReadInt32();
                    var name = r.ReadString();

                    Console.WriteLine(flags1.ToString("X") + flags2.ToString("X") + " " + name + " " + dataOffset.ToString("X"));

                    r.Seek(dataOffset);
                    if ((flags1 & 0xFF00) == 0x0100)
                    {
                        for (int j = 0; j < 0x60 / 4; j++)
                        {
                            var off = r.ReadUInt32();
                            if (off == 0)
                                continue;
                            Console.WriteLine("\t" + j + " " + r.ReadString((uint)(off + 4), -1));
                        }
                    }
                    if ((flags1 & 0xFF00) == 0x0200)
                    {
                        Console.WriteLine("\t" + r.ReadString((uint)(r.ReadInt32() + 4), -1));
                        r.ReadInt32(); // 0
                        var someCount = r.ReadInt32();
                        var someOffset = r.ReadUInt32();
                        var bufferCount = r.ReadInt32();
                        var bufferOffset = r.ReadUInt32();

                        Console.WriteLine("\t" + ((someOffset - bufferOffset) / bufferCount).ToString("X"));
                        Console.WriteLine("\t" + someCount + " " + someOffset.ToString("X"));
                        Console.WriteLine("\t" + bufferCount + " " + bufferOffset.ToString("X"));

                        r.Seek(someOffset);
                        for (int j = 0; j < someCount; j++)
                        {
                            var flag = r.ReadInt32();
                            var offset = r.ReadUInt32();
                            var temp2 = r.Position;
                            r.Seek(offset);
                            ParseSubMesh(r);
                            r.Seek(temp2);
                        }
                    }

                    r.Seek(temp);
                }
            }
        }

        private void ParseSubMesh(DataReader r)
        {
            GenericMesh mesh = new GenericMesh();
            Model.Meshes.Add(mesh);

            Console.WriteLine(r.Position.ToString("X"));

            //0x3C size
            // 5 floats
            r.Position += 4 * 5;
            int index1 = r.ReadInt32();
            int index2 = r.ReadInt32();
            r.ReadInt32(); // 0
            r.ReadInt32(); // 1
            var someOffset1 = r.ReadUInt32(); // 0x38 structure
            var someOffset2 = r.ReadUInt32(); // 0x8 structure flag->offset to 0x60 structure
            r.ReadInt32(); // 0
            var skinDataOffset = r.ReadUInt32();
            mesh.Name = r.ReadString(r.ReadUInt32() + 4, -1);
            r.ReadInt32(); // 0x1C structure
            
            Console.WriteLine("\t\t" + mesh.Name + " " + someOffset1.ToString("X") + " " + someOffset2.ToString("X") + " " + skinDataOffset.ToString("X"));


            r.Seek(someOffset2);

            r.ReadInt32(); // flag or count
            r.Seek(r.ReadUInt32());
            
            var vectorSize = r.ReadInt16();
            var vectorCount = r.ReadInt16();
            var vectorOffset = r.ReadUInt32();

            Console.WriteLine(vectorOffset.ToString("x") + " " + vectorCount.ToString("X"));

            r.Seek(vectorOffset);
            for(int i = 0; i < vectorCount; i++)
            {
                var vert = new GenericVertex();
                vert.Pos = new Vector3(r.ReadInt16() / (float)short.MaxValue, r.ReadInt16() / (float)short.MaxValue, r.ReadInt16() / (float)short.MaxValue) * 100;
                vert.Nrm = new Vector3(r.ReadInt16() / (float)short.MaxValue, r.ReadInt16() / (float)short.MaxValue, r.ReadInt16() / (float)short.MaxValue);
                mesh.Vertices.Add(vert);
            }

            r.Seek(someOffset1);
            r.Skip(0x1C);
            var dlCount = r.ReadInt16() + 1;
            r.Skip(0xE);
            var displayListPointerListOffset = r.ReadUInt32();
            r.Skip(0x8);

            //TODO: needs attributes to read properly
            r.Seek(displayListPointerListOffset);
            for(int j = 0; j < dlCount; j++)
            {
                var off = r.ReadUInt32();

                var temp = r.Position;
                r.Seek(off);
                var primType = r.ReadByte();
                var primCount = r.ReadInt16();

                for(int k = 0; k < primCount; k++)
                {
                    //var index = r.ReadByte(); // TODO: not always bytes
                }

                r.Seek(temp);
            }

            mesh.Optimize();
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }
    }
}
