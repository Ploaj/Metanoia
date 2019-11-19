using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using OpenTK;

namespace Metanoia.Formats.WiiU
{
    /// <summary>
    /// Mario Party 10, Animal Crossing Parade, Wii U Party
    /// </summary>
    public class BNFM : I3DModelFormat
    {
        public string Name => "BNFM";
        public string Extension => ".bnfm";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        private GenericSkeleton Skeleton = new GenericSkeleton();

        private GenericModel Generic = new GenericModel();

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = true;

                r.Skip(0xC); // magic

                var faceOffset = r.ReadUInt32();
                var faceLength = r.ReadInt32();
                var vertexLength = r.ReadInt32();
                var boneTableOffset = r.ReadUInt32();
                r.ReadInt32(); // some flag usually 1

                var vertOffset = r.ReadUInt32();
                var faceOffset2 = r.ReadUInt32();
                r.ReadInt32(); // some flag usually 1
                var nameCount = r.ReadInt32();

                var boneCount = r.ReadInt32();
                var polyCount = r.ReadInt32();
                var matLength = r.ReadInt32();
                var polyNameCount2 = r.ReadInt32();

                var polyNameCount3 = r.ReadInt32();
                var boneTableCount = r.ReadInt32();
                var boneTableCount2 = r.ReadInt32();
                var stringCount = r.ReadInt32();

                var UnkOffset = r.ReadInt32();
                var unk = r.ReadInt32();
                var boneOffset = r.ReadUInt32();
                var polyInfoOffset = r.ReadUInt32();

                var materialOffset = r.ReadUInt32();
                var unkOffset2 = r.ReadUInt32();
                var materialOffset2 = r.ReadUInt32();
                var matrixOffset = r.ReadUInt32();

                r.ReadInt32(); //  same matrix offset
                var stringOffset = r.ReadInt32();

                r.Seek(boneOffset);
                for(int i = 0; i < boneCount; i++)
                {
                    GenericBone bone = new GenericBone();
                    bone.Name = r.ReadString(r.ReadUInt32(), -1);
                    r.ReadInt32(); // hash
                    var parentName = r.ReadString(r.ReadUInt32(), -1);
                    r.ReadInt32(); // hash

                    r.Skip(0x14); // various unimportant stuff

                    var pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    var sca = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                    
                    r.Skip(0x14); // various unimportant stuff

                    var inverseMatrix = new Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                    var matrix = new Matrix4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle(),
                        r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

                    r.Skip(0xC); // various unimportant stuff

                    bone.Transform = inverseMatrix.Inverted();
                    bone.Position = pos;

                    bone.ParentIndex = Skeleton.Bones.FindIndex(e=>e.Name == parentName);

                    Skeleton.Bones.Add(bone);
                }

                r.Seek(materialOffset);
                for(int i = 0; i < matLength / 6; i++)
                {
                    GenericMaterial mat = new GenericMaterial();

                    r.PrintPosition();

                    var name = r.ReadString(r.ReadUInt32(), -1);
                    if (name == "")
                        name = "null";
                    r.Skip(0x110);
                    mat.TextureDiffuse = r.ReadString(r.ReadUInt32(), -1);
                    r.Skip(0x110);

                    if (Generic.MaterialBank.ContainsKey(name))
                        name += "_" + i;

                    if (!Generic.TextureBank.ContainsKey(mat.TextureDiffuse))
                    {
                        Generic.TextureBank.Add(mat.TextureDiffuse, new GenericTexture() { Name = mat.TextureDiffuse });
                    }

                    Generic.MaterialBank.Add(name, mat);
                }

                r.Seek(faceOffset);
                var Indices = new ushort[faceLength / 2];
                for (int i = 0; i < Indices.Length; i++)
                    Indices[i] = r.ReadUInt16();

                r.Seek(vertOffset);
                var Vertices = new GenericVertex[vertexLength / 44];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i] = new GenericVertex()
                    {
                        Pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()),
                        Nrm = new Vector3(r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f),
                        Extra = new Vector4(r.ReadByte()),
                        Clr = new Vector4(r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f),
                        UV0 = new Vector2(r.ReadHalfSingle(), r.ReadHalfSingle()),
                        UV1 = new Vector2(r.ReadHalfSingle(), r.ReadHalfSingle()),
                        Bones = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte()),
                        Weights = new Vector4(r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f),
                        Extra2 = new Vector4(r.ReadSingle(), r.ReadSingle(), 0, 0)
                    };
                }

                var indexOffset = 0;
                var vertexOffset = 0;
                r.Seek(polyInfoOffset);
                for(int i = 0; i < polyCount; i++)
                {
                    GenericMesh mesh = new GenericMesh();

                    mesh.Name = r.ReadString(r.ReadUInt32(), -1);
                    r.ReadInt32();//hash
                    var myboneTableOffset = r.ReadUInt32();
                    var myPolyOffset = r.ReadInt32();

                    r.Skip(8);
                    var indexCount = r.ReadInt32();
                    var vertCount = r.ReadInt32();

                    var boneIDCount = r.ReadInt32();
                    var materialID = r.ReadInt32();
                    r.Skip(8);

                    mesh.MaterialName = Generic.MaterialBank.Keys.ToList()[materialID];

                    // read bone table
                    var temp = r.Position;
                    var boneTable = new int[boneIDCount];
                    r.Seek(myboneTableOffset);
                    for (int j = 0; j < boneIDCount; j++)
                        boneTable[j] = r.ReadInt32();

                    // get indices

                    for (int j = 0; j < indexCount; j++)
                        mesh.Triangles.Add(Indices[indexOffset + j]);
                    indexOffset += indexCount;

                    // get vertices

                    for (int j = 0; j < vertCount; j++)
                    {
                        var vert = Vertices[vertexOffset + j];
                        if(boneIDCount == 1)
                        {
                            vert.Weights = new Vector4(1, 0, 0, 0);
                            vert.Bones = new Vector4(0, 0, 0, 0);
                        }
                        if(boneIDCount != 0)
                        {
                            for (int k = 0; k < 4; k++)
                                if(vert.Weights[k] > 0)
                                    vert.Bones[k] = boneTable[(int)vert.Bones[k]];
                        }
                        if (vert.Weights[0] == 1 && boneIDCount == 1)
                        {
                            vert.Pos = Vector3.TransformPosition(vert.Pos, Skeleton.GetWorldTransform(Skeleton.Bones[(int)vert.Bones[0]]));
                            vert.Nrm = Vector3.TransformNormal(vert.Nrm, Skeleton.GetWorldTransform(Skeleton.Bones[(int)vert.Bones[0]]));
                        }
                        vert.Nrm.Normalize();
                        mesh.Vertices.Add(vert);
                    }
                    vertexOffset += vertCount;

                    r.Seek(temp);

                    //Console.WriteLine(mesh.Name + " " + (myPolyOffset / 2).ToString("X") + " " + myboneTableOffset.ToString("X"));

                    Generic.Meshes.Add(mesh);
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            Generic.Skeleton = Skeleton;

            return Generic;
        }

        public bool Verify(FileItem file)
        {
            return (file.Magic == 0x57550000);
        }
        

        public class PolyInfo
        {
            public string Name;
            public int FaceCount;
            public int PolyVertCount;
            public int BoneIDCount;
            public int MatID;
        }
    }
}
