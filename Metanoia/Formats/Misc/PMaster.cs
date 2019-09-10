using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.Misc
{
    [Format(".lmd")]
    public class PMaster : I3DModelFormat
    {
        private struct AttributeBuffer
        {
            public int Type;
            public int Size;
            public int Offset;
            public int Format;
        }

        private GenericModel Model;

        public string Name => "Pokemon Masters";

        public string Extension => ".lmd";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        public void Open(FileItem File)
        {
            using (DataReader r = new DataReader(File))
            {
                r.BigEndian = false;
                r.Seek(0x34);
                var skelTableOffset = r.Position + r.ReadUInt32();

                Model = new GenericModel();
                Model.Skeleton = new GenericSkeleton();
                r.Seek(skelTableOffset + 8);
                var boneCount = r.ReadInt32();
                for (int i = 0; i < boneCount; i++)
                {
                    var temp = r.Position + 4;
                    r.Seek(r.Position + r.ReadUInt32());
                    //r.PrintPosition();
                    var unk1 = r.ReadInt32();
                    var unk2 = r.ReadInt32();

                    GenericBone bone = new GenericBone();
                    bone.Transform = new OpenTK.Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                    var boneInfoOffset = r.Position + r.ReadUInt32();
                    var boneFlag = r.ReadInt32();
                    var parentCount = r.ReadInt32();
                    var childCount = r.ReadUInt32();
                    r.Skip(4 * childCount); // parent count intofmration
                    
                    r.Seek(boneInfoOffset);
                    var parentName = r.ReadString(r.ReadInt32());
                    r.Skip(1); r.Align(4);
                    bone.Name = r.ReadString(r.ReadInt32());
                    r.Skip(1); r.Align(4);

                    if (parentName != "")
                        bone.ParentIndex = Model.Skeleton.IndexOf(Model.Skeleton.Bones.Find(e => e.Name == parentName));

                    Model.Skeleton.Bones.Add(bone);
                    
                    r.Seek(temp);
                }

                r.Seek(0x48);
                var objectCount = r.ReadInt32();
                var unkOffset = r.Position + r.ReadUInt32();
                var vcount = 0;
                for (int i = 0; i < objectCount - 1; i++)
                {
                    GenericMesh mesh = new GenericMesh();
                    Model.Meshes.Add(mesh);

                    var temp = r.Position + 4;
                    r.Seek(r.Position + r.ReadUInt32());
                    r.Skip(8);
                    mesh.Name = r.ReadString(r.Position + r.ReadUInt32() + 4, -1);
                    var nameOff2 = r.Position + r.ReadUInt32();
                    var parentBoneOff = r.Position + r.ReadUInt32();
                    mesh.MaterialName = r.ReadString(r.Position + r.ReadUInt32() + 12, -1);
                    var transform = new OpenTK.Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                    var boneTableOffset = r.Position + r.ReadUInt32(); // bonetable offset
                    r.ReadInt32(); // some matrix table?

                    r.Skip(0x18); // floats

                    var somecount = r.ReadInt32();
                    var indexTableOffset = r.Position + r.ReadUInt32();
                    var attributeBuffer = r.Position + r.ReadUInt32();
                    var vertCount = r.ReadInt32();
                    var primType = r.ReadInt32();
                    var unkownOffset2 = r.ReadUInt32();

                    if (unkownOffset2 > 0xFFFF)
                        r.ReadInt32();
                    else
                        r.ReadInt16();

                    var t = r.Position;
                    r.Seek(attributeBuffer);
                    var attributes = r.ReadStructArray<AttributeBuffer>(r.ReadInt32());
                    foreach (var a in attributes)
                        Console.WriteLine(a.Format + " " + a.Type + " " + a.Size);
                    r.Seek(t);
                    
                    for (int v = 0; v < vertCount; v++)
                    {
                        var vert = new GenericVertex();

                        foreach (var a in attributes)
                        {
                            float[] values = new float[a.Size];
                            for (int vi = 0; vi < values.Length; vi++)
                            {
                                switch (a.Format)
                                {
                                    case 0x00:
                                        values[vi] = r.ReadSingle();
                                        break;
                                    case 0x07:
                                        values[vi] = r.ReadInt16() / (float)short.MaxValue;
                                        break;
                                    case 0x06:
                                        values[vi] = r.ReadSByte();
                                        break;
                                    case 0x0B:
                                        values[vi] = r.ReadUInt16() / (float)ushort.MaxValue;
                                        break;
                                    case 0x0E:
                                        values[vi] = r.ReadByte() / (float)255;
                                        break;
                                    default:
                                        throw new NotSupportedException("Unknown Attribute Format " + a.Format);
                                }
                            }

                            switch (a.Type)
                            {
                                case 0:
                                    vert.Pos = new OpenTK.Vector3(values[0], values[1], values[2]);
                                    break;
                                case 1:
                                    vert.Clr = new OpenTK.Vector4(values[0], values[1], values[2], values[3]);
                                    break;
                                case 3:
                                    vert.UV0 = new OpenTK.Vector2(values[0], values[1]);
                                    break;
                                case 15:
                                    vert.Bones = new OpenTK.Vector4(values[0], values[1], values[2], values[3]);
                                    break;
                                case 16:
                                    vert.Weights = new OpenTK.Vector4(values[0], values[1], values[2], values[3]);
                                    break;
                                    //default:
                                    //    throw new NotSupportedException("Unknown vertex attribute " + a.Type);
                            }
                        }
                        
                        mesh.Vertices.Add(vert);
                    }

                    r.Seek(indexTableOffset);
                    var indexBufferSize = r.ReadInt32();
                    var indexCount = (indexBufferSize - 6) / 2;
                    if(indexBufferSize > 0xFFFF)
                        r.ReadInt32();
                    else
                        r.ReadInt16();

                    for (int j = 0; j < indexCount; j++)
                    {
                        var v = r.ReadUInt16();
                        mesh.Triangles.Add(mesh.VertexCount < 0xFF ? (uint)(v & 0xFF) : v);
                    }
                    
                    //r.PrintPosition();
                    Console.WriteLine(mesh.Name + " " + mesh.VertexCount.ToString("X") + " " + mesh.Triangles.Min().ToString("X") + " " + mesh.Triangles.Max().ToString("X"));
                    //mesh.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip;

                    r.Seek(temp);
                }
            }
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }

        public bool Verify(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.Seek(4);
                if (r.ReadString(4) == "LMD0")
                    return true;
            }
            return false;
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
