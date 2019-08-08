using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using OpenTK;

namespace Metanoia.Formats.Unity
{
    [Format(Extension = ".mesh", Description = "Unity Mesh Asset")]
    public class UnityMeshImporter : IModelFormat
    {
        private UnityMesh Mesh = new UnityMesh();

        public void Open(FileItem File)
        {
            using (DataReader reader = new DataReader(new FileStream(File.FilePath, FileMode.Open)))
            {
                Mesh.Parse(reader);
            }
        }

        public GenericModel ToGenericModel()
        {
            var model = new GenericModel();

            var verts = GetVertexData();

            int indexOffset = 0;
            int vertexOffset = 0;
            foreach(var m in Mesh.SubMesh)
            {
                var mesh = new GenericMesh();
                mesh.Name = m.FirstVertex + "_Mesh";
                model.Meshes.Add(mesh);

                for (int f = 0; f < m.IndexCount; f++)
                    mesh.Triangles.Add(Mesh.IndexBuffer[indexOffset + f]);

                for (int v = 0; v < m.VertexCount; v++)
                    mesh.Vertices.Add(verts[v+vertexOffset]);

                indexOffset += m.IndexCount;
                vertexOffset += m.VertexCount;
            }



            return model;
        }
        private List<GenericVertex> GetCompVertexData()
        {
            List<GenericVertex> vertices = new List<GenericVertex>();

            var positions = Mesh.CompVertices.GetDecompressedFloats();

            for(int i = 0; i < positions.Length / 3; i++)
            {
                var vert = new GenericVertex();
                vert.Pos = new Vector3(positions[i * 3 + 0], positions[i * 3 + 1], positions[i * 3 + 2]);
                vertices.Add(vert);
            }

            return vertices;
        }

        private List<GenericVertex> GetVertexData()
        {
            List<GenericVertex> vertices = new List<GenericVertex>();

            using (DataReader d = new DataReader(new MemoryStream(Mesh.VertexData.TypelessData)))
            {
                var vertexData = Mesh.VertexData;

                // calucate channel strides
                Dictionary<int, int> channelToStride = new Dictionary<int, int>();
                foreach(var c in vertexData.Channels)
                {
                    if (!channelToStride.ContainsKey(c.Stream))
                    {
                        channelToStride.Add(c.Stream, 0);
                    }

                    channelToStride[c.Stream] = Math.Max(channelToStride[c.Stream], c.Offset + 4 * c.Diminsion);
                }

                for (int i = 0; i < Mesh.VertexData.VertexCount; i++)
                {
                    GenericVertex v = new GenericVertex();
                    int index = 0;
                    foreach (var c in vertexData.Channels)
                    {
                        d.Position = (uint)(i * channelToStride[c.Stream]);
                        if (c.Stream == 1)
                            d.Position += (uint)(channelToStride[0] * vertexData.VertexCount);
                        switch (index)
                        {
                            case 0:
                                if (c.Diminsion != 3 || c.Format != 0)
                                    throw new NotSupportedException();
                                v.Pos = new Vector3(d.ReadSingle(), d.ReadSingle(), d.ReadSingle());
                                //v.Pos = v.Pos.Xzy;
                                break;
                            case 1:
                                if (c.Diminsion != 3 || c.Format != 0)
                                    throw new NotSupportedException();
                                v.Nrm = new Vector3(d.ReadSingle(), d.ReadSingle(), d.ReadSingle());
                                //v.Nrm = v.Nrm.Xzy;
                                break;
                        }
                        index++;
                    }
                    vertices.Add(v);
                }

            }

            return vertices;
        }
    }

    public class UnityMesh
    {
        public string Name { get; set; }

        public List<UnitySubMesh> SubMesh { get; set; } = new List<UnitySubMesh>();

        public List<UnityBlendShapeVertex> BlendShapeVertices { get; set; } = new List<UnityBlendShapeVertex>();

        public List<UnityBlendShape> BlendShapes { get; set; } = new List<UnityBlendShape>();

        public List<UnityBlendShapeChannel> BlendShapeChannels { get; set; } = new List<UnityBlendShapeChannel>();

        public List<float> FullWeights { get; set; } = new List<float>();

        public List<Matrix4> BindPoses { get; set; } = new List<Matrix4>();

        public List<int> BoneNameHashes { get; set; } = new List<int>();

        public int RootBoneNameHash { get; set; }

        public byte MeshCompression { get; set; }

        public bool IsReadable { get; set; }

        public bool KeepVertices { get; set; }

        public bool KeepIndices { get; set; }

        public List<ushort> IndexBuffer { get; set; } = new List<ushort>();

        public List<UnityBoneInfluence> BoneInfluences { get; set; } = new List<UnityBoneInfluence>();

        public UnityVertexData VertexData = new UnityVertexData();

        //TODO: compressed mesh
        public CompressedMesh CompVertices = new CompressedMesh();

        public void Parse(DataReader reader)
        {
            Name = reader.ReadString(reader.ReadInt32());
            if(reader.BaseStream.Position % 4 != 0)
                reader.BaseStream.Position += 4 - (reader.BaseStream.Position % 4);

            int subMeshCount = reader.ReadInt32();
            for(int i =0; i < subMeshCount; i++)
            {
                UnitySubMesh sm = new UnitySubMesh();
                sm.Parse(reader);
                SubMesh.Add(sm);
            }
            
            int blendCount = reader.ReadInt32();
            for (int i = 0; i < blendCount; i++)
            {
                var sm = new UnityBlendShapeVertex();
                sm.Parse(reader);
                BlendShapeVertices.Add(sm);
            }

            int blendShapeCount = reader.ReadInt32();
            for (int i = 0; i < blendShapeCount; i++)
            {
                var sm = new UnityBlendShape();
                sm.Parse(reader);
                BlendShapes.Add(sm);
            }
            int blendShapeChannelCount = reader.ReadInt32();
            for (int i = 0; i < blendShapeChannelCount; i++)
            {
                var sm = new UnityBlendShapeChannel();
                sm.Parse(reader);
                BlendShapeChannels.Add(sm);
            }
            int fullWeightCount = reader.ReadInt32();
            for (int i = 0; i < fullWeightCount; i++)
            {
                var sm = reader.ReadSingle();
                FullWeights.Add(sm);
            }
            
            int bindPoseCount = reader.ReadInt32();
            for (int i = 0; i < bindPoseCount; i++)
            {
                var sm = new Matrix4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                    reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                BindPoses.Add(sm);
            }
            
            int boneNameHashCount = reader.ReadInt32();
            for (int i = 0; i < boneNameHashCount; i++)
            {
                BoneNameHashes.Add(reader.ReadInt32());
            }

            RootBoneNameHash = reader.ReadInt32();
            MeshCompression = reader.ReadByte();
            IsReadable = reader.ReadByte() == 1;
            KeepVertices = reader.ReadByte() == 1;
            KeepIndices = reader.ReadByte() == 1;

            int IndexBufferCount = reader.ReadInt32();
            for (int i = 0; i < IndexBufferCount / 2; i++)
            {
                IndexBuffer.Add(reader.ReadUInt16());
            }

            int BoneICount = reader.ReadInt32();
            for (int i = 0; i < BoneICount; i++)
            {
                var bi = new UnityBoneInfluence();
                bi.Parse(reader);
                BoneInfluences.Add(bi);
            }
            
            VertexData.Parse(reader);

            reader.PrintPosition();

            CompVertices.Parse(reader);

            reader.PrintPosition();
        }
        
    }

    public class UnitySubMesh
    {
        public int FirstByte { get; set; }
        public int IndexCount { get; set; }
        public int Topology { get; set; }
        public int FirstVertex { get; set; }
        public int VertexCount { get; set; }
        public float[] AABB { get; set; } = new float[6];

        public void Parse(DataReader reader)
        {
            FirstByte = reader.ReadInt32();
            IndexCount = reader.ReadInt32();
            Topology = reader.ReadInt32();
            FirstVertex = reader.ReadInt32();
            VertexCount = reader.ReadInt32();
            for (int i = 0; i < AABB.Length; i++)
                AABB[i] = reader.ReadSingle();
        }
    }

    public class UnityBlendShapeVertex
    {
        public int Index { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector3 Tangent { get; set; }

        public void Parse(DataReader reader)
        {
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Normal = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Tangent = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Index = reader.ReadInt32();
        }
    }

    public class UnityBlendShape
    {
        public int FirstVertex { get; set; }
        public int Vertexcount { get; set; }
        public bool HasNormals { get; set; }
        public bool HasTangents { get; set; }

        public void Parse(DataReader reader)
        {
            FirstVertex = reader.ReadInt32();
            Vertexcount = reader.ReadInt32();
            HasNormals = reader.ReadByte() == 1;
            HasTangents = reader.ReadByte() == 1;
            reader.ReadByte();
            reader.ReadByte();
        }
    }

    public class UnityBlendShapeChannel
    {
        public string Name { get; set; }
        public int NameHash { get; set; }
        public int FrameIndex { get; set; }
        public int FrameCount { get; set; }
        
        public void Parse(DataReader reader)
        {
            Name = reader.ReadString(reader.ReadInt32());
            if((reader.Position % 4) != 0)
                reader.Position += 4 - (reader.Position % 4);
            NameHash = reader.ReadInt32();
            FrameIndex = reader.ReadInt32();
            FrameCount = reader.ReadInt32();
        }
    }

    public class UnityFullWeights
    {
        public float[] Values { get; set; }

        public void Parse(DataReader reader)
        {
            Values = new float[reader.ReadInt32()];
            for (int i = 0; i < Values.Length; i++)
                Values[i] = reader.ReadSingle();
        }
    }

    public class UnityBoneInfluence
    {
        public Vector4 Bones;
        public Vector4 Weights;

        public void Parse(DataReader reader)
        {
            Weights = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Bones = new Vector4(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
    }

    public class UnityVertexData
    {
        public int CurrentChannels { get; set; }
        public int VertexCount { get; set; }
        public List<UnityChannelInfo> Channels { get; set; } = new List<UnityChannelInfo>();

        public byte[] TypelessData { get; set; }

        public void Parse(DataReader reader)
        {
            CurrentChannels = reader.ReadInt32();
            VertexCount = reader.ReadInt32();
            int channelCount = reader.ReadInt32();
            for(int i =0; i < channelCount; i++)
            {
                var c = new UnityChannelInfo();
                c.Parse(reader);
                Channels.Add(c);
            }

            TypelessData = reader.ReadBytes(reader.ReadInt32());
        }
    }

    public class UnityChannelInfo
    {
        public byte Stream { get; set; }
        public byte Offset { get; set; }
        public byte Format { get; set; }
        public byte Diminsion { get; set; }

        public void Parse(DataReader reader)
        {
            Stream = reader.ReadByte();
            Offset = reader.ReadByte();
            Format = reader.ReadByte();
            Diminsion = reader.ReadByte();
        }
    }

    public class CompressedMesh
    {
        public int Count { get; set; }
        public float Range { get; set; }
        public float Start { get; set; }
        public byte[] Buffer { get; set; }
        public int BitCount { get; set; }

        public void Parse(DataReader reader)
        {
            Count = reader.ReadInt32();
            Range = reader.ReadSingle();
            Start = reader.ReadSingle();
            Buffer = reader.ReadBytes(reader.ReadInt32());
            if ((reader.Position % 4) != 0)
                reader.Position += 4 - (reader.Position % 4);
            BitCount = reader.ReadInt32();
        }

        public float[] GetDecompressedFloats()
        {
            List<float> output = new List<float>();

            using (DataReader r =  new DataReader(new MemoryStream(Buffer)))
            {
                int mask = 0;
                for(int i = 0; i < BitCount; i++)
                {
                    mask |= (1 << i);
                }

                for (int i = 0; i < Count; i++)
                {
                    var quan = r.ReadBits(BitCount);
                    var value = Start + Range * (quan / (float)mask);
                    output.Add(value);
                }
            }

            return output.ToArray();
        }
    }
    
}
