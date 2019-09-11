using System;
using System.Collections.Generic;
using Metanoia.Modeling;
using System.IO;
using OpenTK;

namespace Metanoia.Formats.Misc
{
    public class PTE : I3DModelFormat
    {
        public string Name => "Punch Time Explosion";

        public string Extension => ".wii";

        public string Description => "Wii Version";

        public bool CanOpen => true;

        public bool CanSave => false;

        private struct VapsHeader
        {
            public int Magic;
            public short MajorVersion;
            public short MinorVersion;
            public int ChildrenCount; // ??
            public int TotalSize; // including sub children
            public int TotalSizeChildren; // only children
            public int ContentType;
            public int Unk2;
            public int DataStart; // relative to start of vaps
            public int Count1;
            public int Count2;
            public int Unk3;
            public int Unk4;
            public int Unk5;
            public int flags; //??
            public int Unk6;
            public int Unk7;
        }

        private struct DataOffset
        {
            public int Unk;
            public int Size;
            public uint Offset; // relative to DataStart
        }

        private class VAPS
        {
            public int ContentType;
            public uint DataStart;
            public int[] Indices;
            public DataOffset[] Offsets;

            public uint this[int i]
            {
                get { return Offsets[i].Offset + DataStart; }
            }
        }

        private struct PolyStruct
        {
            public int PrimType; //??
            public short Unk1;
            public short FOffset;
            public short FCount;
            public short Unk2;
            public int Unk4;
            public uint SectionSize;
            public int Unk5;
            public int Unk6;
            public int Unk7;
            public int Unk8;
        }

        private GenericSkeleton Skeleton = new GenericSkeleton();

        private List<GenericVertex> Vertices = new List<GenericVertex>();

        private List<GenericMesh> Meshes = new List<GenericMesh>();

        public void Open(FileItem File)
        {
            using (DataReader reader = new DataReader(new MemoryStream(File.GetFileBinary())))
            {
                reader.BigEndian = true;

                var header = ParseVAPS(reader);

                uint polyOff = 0;

                for (int h = 0; h < header.Offsets.Length; h++)
                {
                    reader.Seek(header[h]);
                    var contentHeader = ParseVAPS(reader);

                    switch (contentHeader.ContentType)
                    {
                        case 0x0C: // Bones
                            var boneCount = reader.ReadInt16();
                            reader.Position += 0xA;

                            for (int i = 0; i < boneCount; i++)
                            {
                                GenericBone b = new GenericBone();
                                b.Name = "Bone_" + i;
                                Skeleton.Bones.Add(b);
                                b.QuaternionRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                b.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                b.Scale = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                b.ParentIndex = reader.ReadSByte();
                                reader.Position += 3;
                            }
                            break;
                        case 0x03: // textures
                            Console.WriteLine(contentHeader.Indices.Length + " " + contentHeader.Offsets.Length);
                            break;
                        case 0x04: // mesh objects
                            polyOff = header[h];
                            break;
                        case 0x09: // uv buffer

                            break;
                        case 0x0E: // materials?

                            break;
                        case 0x0D: // sounds?

                            break;
                        case 0x16: // vertices
                            ReadVertices(reader);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine("Unknown content type " + contentHeader.ContentType.ToString("X") + " " + reader.Position.ToString("X"));
                            break;
                    }
                }
                //Polygons
                reader.Seek(polyOff);
                ParseVAPS(reader);
                ReadPolygons(reader);
            }
        }

        private void ReadPolygons(DataReader reader)
        {
            uint polyStartOffset = reader.Position;
            reader.Position += 0x14;
            uint uvOffset = polyStartOffset + reader.ReadUInt32();
            uint triangleOffset = polyStartOffset + reader.ReadUInt32();
            uint uvTriangledataSize = polyStartOffset + reader.ReadUInt32();
            int polyCount = reader.ReadInt16();
            reader.ReadInt16();
            uint polyOffset = polyStartOffset + reader.ReadUInt32();

            reader.Seek(polyOffset);
            for (int i = 0; i < polyCount; i++)
            {
                reader.PrintPosition();
                var polyStruct = reader.ReadStruct<PolyStruct>();

                var mesh = new GenericMesh();
                Meshes.Add(mesh);
                mesh.Name = "Mesh_" + i;

                var temp = reader.Position;
                reader.Position = (uint)(triangleOffset + 3);
                Console.WriteLine(mesh.Name);

                for (int j = 0; j < polyStruct.FCount; j++)
                {
                    var v = new GenericVertex();
                    var pos = Vertices[reader.ReadInt16()];
                    v.Pos = pos.Pos;
                    v.Nrm = pos.Nrm;
                    v.Bones = pos.Bones;
                    v.Weights = pos.Weights;

                    //TODO: uv and ?color?
                    reader.ReadInt16(); // uv
                    reader.ReadInt16(); // color

                    mesh.Vertices.Add(v);
                }

                mesh.Optimize();

                reader.Position = temp;

                triangleOffset += polyStruct.SectionSize;
            }
        }

        private void ReadVertices(DataReader reader)
        {
            Matrix4 mat = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            reader.Position += 0x10;
            int weight1Group = reader.ReadByte() - 1;
            int weight2Group = reader.ReadByte() - 1;
            int weight3Group = reader.ReadByte() - 1;
            int weight4Group = reader.ReadByte() - 1;
            reader.Position += 0x08;
            int weight1Size = reader.ReadByte() + 1;
            int weight2Size = reader.ReadByte() + 1;
            int weight3Size = reader.ReadByte() + 1;
            int weight4Size = reader.ReadByte() + 1;

            ReadBuffer(reader, weight1Group, weight1Size, 1);
            ReadBuffer(reader, weight2Group, weight2Size, 2);
            ReadBuffer(reader, weight3Group, weight3Size, 3);
            ReadBuffer(reader, weight4Group, weight4Size, 4);
        }
        

        private void ReadBuffer(DataReader reader, int count, int extraCount, int weightCount)
        {
            if (count < 0)
                return;
            reader.PrintPosition();
            int stride = 0x18;
            if (weightCount == 1)
                stride += 4;
            else
            if (weightCount == 2 || weightCount == 3)
                stride += 6 * weightCount;
            else
                stride += 4 * weightCount + weightCount;

            for(int i = 0; i < count; i++)
            {
                for(int j = 0; j < 0x1000 / stride; j++)
                {
                    Vertices.Add(ReadVertex(reader, weightCount));
                }
                reader.Position += (uint)(0x1000 % stride);
            }
            
            for (int i = 0; i < extraCount; i++)
            {
                Vertices.Add(ReadVertex(reader, weightCount));
            }
            uint pad = 0x20 - ((uint)(extraCount * stride) % 0x20);
            if(pad != 0x20)
                reader.Position += pad;
        }

        private GenericVertex ReadVertex(DataReader reader, int weightCount)
        {
            GenericVertex v = new GenericVertex();

            v.Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            v.Nrm = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            if (weightCount == 1)
            {
                v.Bones = new Vector4(reader.ReadInt32(), 0, 0, 0);
                v.Weights = new Vector4(1, 0, 0, 0);
            }
            else
            if (weightCount == 2)
            {
                for (int w = 0; w < weightCount; w++)
                    v.Weights[w] = reader.ReadSingle();
                for (int w = 0; w < weightCount; w++)
                    v.Bones[w] = reader.ReadInt16();
            }
            else
            if (weightCount == 4)
            {
                for (int w = 0; w < weightCount; w++)
                    v.Weights[w] = reader.ReadSingle();
                for (int w = 0; w < weightCount; w++)
                    v.Bones[w] = reader.ReadByte();
            }
            return v;
        }

        private VAPS ParseVAPS(DataReader reader)
        {
            var start = reader.Position;
            var vap = new VAPS();
            var mainHeader = reader.ReadStruct<VapsHeader>();
                vap.ContentType = mainHeader.ContentType;
             vap.Indices = new int[mainHeader.Count1];
            for (int i = 0; i < vap.Indices.Length; i++)
                vap.Indices[i] = reader.ReadInt32();
            vap.Offsets = reader.ReadStructArray<DataOffset>(mainHeader.Count2);
            int dataStart = (int)start + mainHeader.DataStart;
            vap.DataStart = (uint)dataStart;
            reader.Seek(vap.DataStart);
            return vap;
        }

        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();

            model.Skeleton = Skeleton;

            model.Meshes = Meshes;

            return model;
        }

        public bool Verify(FileItem file)
        {
            return file.Extension.Equals(Extension);
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
