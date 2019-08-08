using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using OpenTK;

namespace Metanoia.Formats.Misc
{
    [Format(Extension = ".pac", Description = "Kill La Kill IF")]
    public class KillPac : IModelFormat
    {
        private GenericModel model = new GenericModel();

        public void Open(FileItem File)
        {
            ArcPAC pac = new ArcPAC(System.IO.File.ReadAllBytes(File.FilePath));

            foreach(var file in pac.Files)
            {
                if(file.Type == "MDLD")
                {
                    ParseMDLD(file.Data);
                }
            }
        }

        private void ParseMDLD(byte[] data)
        {
            using (DataReader r = new DataReader(data))
            {
                r.BigEndian = false;

                r.ReadInt32(); // magic
                var offset1 = r.ReadUInt32();
                var polyInfoOffset = r.ReadUInt32();
                var polyInfoCount = r.ReadInt32();
                var boneOffset = r.ReadUInt32();
                var boneCount = r.ReadInt32();
                var bufferOffset = r.ReadUInt32();
                var bufferLength = r.ReadInt32();
                var vertexCount = r.ReadInt32();

                var att = r.ReadString(0x68, -1).Split('_');

                r.Seek(boneOffset);
                model.Skeleton = new GenericSkeleton();
                for(int i = 0; i < boneCount; i++)
                {
                    var bone = new GenericBone();
                    bone.Name = r.ReadString(r.Position, -1);
                    r.Skip(0x20);
                    bone.Transform = Matrix4.Identity;
                    bone.QuaternionRotation = new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    bone.Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    bone.Scale = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    bone.ParentIndex = r.ReadInt32();
                    model.Skeleton.Bones.Add(bone);
                }
                model.Skeleton.TransformWorldToRelative();

                List<GenericVertex> vertices = new List<GenericVertex>();
                for(uint i = 0; i < vertexCount; i++)
                {
                    r.Seek(bufferOffset + 60 * i);
                    var vert = new GenericVertex();
                    var stop = false;
                    foreach (var va in att)
                    {
                        switch (va)
                        {
                            case "vp3":
                                vert.Pos = new OpenTK.Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                break;
                            case "vc":
                                vert.Clr = new OpenTK.Vector4(r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f);
                                break;
                            case "vn":
                                vert.Nrm = new OpenTK.Vector3(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                r.ReadHalfSingle();
                                break;
                            case "vt":
                                vert.UV0 = new OpenTK.Vector2(r.ReadHalfSingle(), r.ReadHalfSingle());
                                break;
                            case "von":
                                r.Skip(8);
                                break;
                            case "vb4":
                                vert.Bones = new OpenTK.Vector4(r.ReadInt16(), r.ReadInt16(), r.ReadInt16(), r.ReadInt16());
                                vert.Weights = new OpenTK.Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                break;
                            case "vs":
                                break;
                            default:
                                stop = true;
                                break;
                        }
                        if (stop)
                            break;
                    }
                    vertices.Add(vert);
                }

                r.Seek(polyInfoOffset);
                for(int i = 0; i < polyInfoCount; i++)
                {
                    GenericMesh mesh = new GenericMesh();

                    model.Meshes.Add(mesh);

                    mesh.Name = r.ReadString(r.Position, -1);
                    r.Skip(0x40);
                    var polyBufferOffset = r.ReadUInt32();
                    var polyBufferLength = r.ReadUInt32();
                    var polyBufferCount = r.ReadUInt32();
                    var primType = r.ReadInt32();
                    r.Skip(0x60);

                    var temp = r.Position;
                    r.Seek(polyBufferOffset);
                    for(int j = 0; j < polyBufferCount; j++)
                    {
                        if(primType == 4)
                        {
                            mesh.Vertices.Add(vertices[r.ReadInt32()]);
                        }
                    }
                    mesh.Optimize();
                    r.Seek(temp);

                }
            }
        }

        public GenericModel ToGenericModel()
        {
            return model;
        }
    }

    public class ArcPAC
    {
        public class ArcFile
        {
            public string Name;
            public string Type;
            public byte[] Data;
        }

        public List<ArcFile> Files = new List<ArcFile>();

        public ArcPAC(byte[] data)
        {
            using (DataReader r  = new DataReader(data))
            {
                r.BigEndian = false;
                r.Skip(12); // magic + version
                int count = r.ReadInt32();
                uint offset = r.ReadUInt32();

                r.Seek(offset);
                for(int i = 0; i < count; i++)
                {
                    var file = new ArcFile();
                    Files.Add(file);
                    file.Type = r.ReadString(4);
                    r.ReadInt32(); // flag
                    var fsize = r.ReadInt32();
                    var foffset = r.ReadUInt32();
                    file.Name = r.ReadString(r.Position, -1);
                    r.Skip(0x80);
                    file.Data = r.GetSection(foffset, fsize);
                }
            }
        }
    }
}
