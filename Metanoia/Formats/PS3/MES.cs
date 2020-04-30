using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.PS3
{
    /// <summary>
    /// TODO: incomplete
    /// </summary>
    public class MES : I3DModelFormat
    {
        public string Name => "";

        public string Extension => ".mes";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        private GenericModel model = new GenericModel();


        public void Open(FileItem file)
        {
            model.Skeleton = new GenericSkeleton();
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = false;

                r.Seek(0xCC);

                var somecount = r.ReadInt32();
                var section1Size = r.ReadInt32();
                var section2Size = r.ReadInt32();
                r.ReadInt16();
                var section3Size = r.ReadInt32();
                r.Skip(0x0A);
                var section4Size = r.ReadInt32();
                var section5Size = r.ReadInt32();
                r.Skip(0x18);
                r.Skip((uint)section1Size);

                var mesh = new GenericMesh();
                mesh.Name = "mesh1";
                model.Meshes.Add(mesh);
                for (int i = 0; i < section2Size / 4; i++)
                {
                    var v1 = r.ReadSByte();
                    var v2 = r.ReadSByte();
                    var v3 = r.ReadSByte();
                    var v4 = (float)r.ReadByte()/255f;

                    var v = new GenericVertex()
                    {
                        Pos = new OpenTK.Vector3(v1, v2, v3) * v4,
                        Clr = new OpenTK.Vector4(1, 1, 1, 1)
                    };
                    if (float.IsNaN(v.Pos.X) || float.IsInfinity(v.Pos.Y))
                        v.Pos = new OpenTK.Vector3();
                    mesh.Vertices.Add(v);
                    Console.WriteLine(v.Pos.ToString());
                }
                mesh.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip;
                for (uint i = 0; i < mesh.Vertices.Count; i++)
                    mesh.Triangles.Add(i);

                for(int i = 0; i < section3Size / 12; i++)
                {
                    Console.WriteLine(r.ReadHalfSingle() + " " + r.ReadHalfSingle() + " " + r.ReadHalfSingle()
                        + " " + r.ReadInt16().ToString("X") + " " + r.ReadHalfSingle() + " " + r.ReadHalfSingle());
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return model;
        }

        public bool Verify(FileItem file)
        {
            return (file.MagicString == "MESH");
        }
    }
}
