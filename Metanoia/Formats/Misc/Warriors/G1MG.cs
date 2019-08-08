using Metanoia.Modeling;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats.Misc
{
    public enum G1MVertexAttribute
    {
        POSITION = 0x0000,
        WEIGHT,
        BONES,
        NORMALS,
        UNK1,
        TEXCOORD0,
        TANGENT,
        BINORMAL,
        COLOR = 0x000A,
        FOG = 0x000B,
    }

    public class G1MG
    {
        private string type;

        public class G1MBuffer
        {
            public byte[] Buffer;
            public int Stride;
            public int Count;
        }

        public class G1MPolygon
        {
            public string Name;
            public int FaceOffset = 0;
            public int FaceCount = 0;
            public int TextureBankIndex;
            public int MaterialIndex;
            public int Buffer;
            public int TextureCount;
            public int BoneTableIndex;
            public int relatedToBones;
            public int UnknownIndex;
        }

        public struct G1MAttribute
        {
            public int bufferindex;
            public int offset;
            public int datatype;
            public int semantic;
            public int track;
        }

        public class G1TextureBank
        {
            public int DiffuseTextureIndex;
        }

        public class G1Material
        {
            public Dictionary<string, object> Parameters = new Dictionary<string, object>();

            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                foreach(var v in Parameters)
                {
                    if (v.Value == null)
                        b.AppendLine(v.Key);
                    else
                    if(v.Value is float[] floats)
                        b.AppendLine(v.Key + ": " + string.Join(", ", floats));
                    else
                        b.AppendLine(v.Key + ": " + v.Value.ToString());
                }
                return b.ToString();
            }
        }

        public class G1LOD
        {
            public List<G1LODGroup> Groups = new List<G1LODGroup>();
        }

        public class G1LODGroup
        {
            public string Name;
            public int ID;
            public int ID2;
            public List<int> Indices = new List<int>();
        }

        public Dictionary<int, int[]> BindMatches = new Dictionary<int, int[]>();
        public List<List<G1MAttribute>> Attributes = new List<List<G1MAttribute>>();
        public List<G1MPolygon> Polygons = new List<G1MPolygon>();
        public List<G1MBuffer> Buffers = new List<G1MBuffer>();
        public List<G1TextureBank> TextureBanks = new List<G1TextureBank>();
        public List<G1Material> Materials = new List<G1Material>();
        public List<G1LOD> Lods = new List<G1LOD>();
        public List<ushort[]> IndexBuffers = new List<ushort[]>();

        public G1MG(DataReader r)
        {
            type = new string(r.ReadChars(3));
            r.Skip(1);

            // Bounding Volume
            Console.WriteLine(r.ReadSingle() + " " + r.ReadSingle() + " " + r.ReadSingle() + " " + r.ReadSingle());
            Console.WriteLine(r.ReadSingle() + " " + r.ReadSingle() + " " + r.ReadSingle());

            if (type == "DX1" || type == "NX_")
            {
                ReadMaterial(r);

                ReadBuffers(r);

                ReadAttribtues(r);

                ReadBoneBinds(r);

                ReadIndexBuffers(r);

                ReadPolygons(r);

                ReadPolyInfo(r);
                r.PrintPosition();
            }

        }

        private void ReadPolyInfo(DataReader r)
        {
            r.Skip(0x8);
            int LodCount = r.ReadInt32();

            for (int lod = 0; lod < LodCount; lod++)
            {
                G1LOD glod = new G1LOD();
                Lods.Add(glod);

                // header 24?
                r.Skip(0x0C);
                var meshCount = r.ReadInt32();
                meshCount += r.ReadInt32();
                r.Skip(0x10);

                for (int m = 0; m < meshCount; m++)
                {
                    G1LODGroup group = new G1LODGroup();
                    glod.Groups.Add(group);
                    group.Name = r.ReadString(r.Position, -1);
                    r.Skip(0x10); //name and other stuff
                    group.ID = r.ReadInt32();
                    group.ID2 = r.ReadInt32();
                    var indices = r.ReadInt32();
                    Console.WriteLine(group.Name + " " + group.ID + " " + group.ID2 + " " + indices);
                    if (indices > 0)
                    {
                        for (int i = 0; i < indices; i++)
                            group.Indices.Add(r.ReadInt32());
                    }
                    else
                        r.Skip(4);
                }
            }
        }

        private void ReadPolygons(DataReader r)
        {
            Console.WriteLine(r.Position.ToString("x"));
            r.Skip(4);
            uint next = r.Position + r.ReadUInt32() - 4;

            int count = r.ReadInt32();

            var faceoffset = 0;

            //List<int> DesOfRoot = DecendentsOf(Root);
            for (int i = 0; i < count; i++)
            {
                G1MPolygon p = new G1MPolygon();
                var un = r.ReadInt32();
                p.UnknownIndex = r.ReadInt32();
                p.BoneTableIndex = r.ReadInt32();
                p.relatedToBones = r.ReadInt32();
                p.TextureCount = r.ReadInt32();
                p.MaterialIndex = r.ReadInt32();
                p.TextureBankIndex = r.ReadInt32();// Materials[];
                p.Buffer = r.ReadInt32();
                r.Skip(0x4);
                r.Skip(0x4); // format 1 = triangle 4 = strips
                r.Skip(0x8); // vertex offset and count
                p.FaceOffset = r.ReadInt32();
                p.FaceCount = r.ReadInt32();
                faceoffset += p.FaceCount;
                p.Name = "Polygon_" + i;
                Polygons.Add(p);
            }
            r.Seek(next);
        }

        private void ReadIndexBuffers(DataReader r)
        {
            r.Skip(4);
            var size = r.ReadInt32();
            int IndexBufferCount = r.ReadInt32();

            if (size - 0xC > 0)
            {
                for (int c = 0; c < IndexBufferCount; c++)
                {
                    int fCount = r.ReadInt32();
                    var indBuffer = new ushort[fCount];
                    Console.WriteLine(r.ReadInt32() + " " + r.ReadInt32());
                    for (int i = 0; i < fCount; i++)
                    {
                        indBuffer[i] = r.ReadUInt16();
                    }
                    IndexBuffers.Add(indBuffer);
                }
            }
        }

        private void ReadBoneBinds(DataReader r)
        {
            r.PrintPosition();
            //Bone Binds
            {
                r.Skip(4);
                int Size = r.ReadInt32();

                int Count = r.ReadInt32();


                for (int i = 0; i < Count; i++)
                {
                    int c = r.ReadInt32();
                    int[] binds = new int[c];
                    for (int j = 0; j < c; j++)
                    {
                        int index = r.ReadInt32();
                        int te = r.ReadInt32();
                        int BoneIndex = r.ReadInt16();
                        int fl8 = r.ReadInt16();
                        binds[j] = BoneIndex;
                    }
                    BindMatches.Add(i, binds);
                }
            }
        }

        private void ReadAttribtues(DataReader r)
        {
            // Attributes
            r.Skip(4);
            int Size = r.ReadInt32();

            int count = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                Console.WriteLine("Attr start: " + r.Position.ToString("x"));
                int BuffCount = r.ReadInt32();
                r.Skip((uint)BuffCount * 4);
                int AttCount = r.ReadInt32();

                List<G1MAttribute> Atts = new List<G1MAttribute>();
                for (int j = 0; j < AttCount; j++)
                {
                    Atts.Add(new G1MAttribute()
                    {
                        bufferindex = r.ReadInt16(),
                        offset = r.ReadInt16(),
                        datatype = r.ReadInt16(),
                        semantic = r.ReadByte(),
                        track = r.ReadByte(),
                    });
                    Console.WriteLine(j + " " + (G1MVertexAttribute)Atts[j].semantic + " " + Atts[j].datatype + " " + Atts[j].track + " " + Atts[j].offset.ToString("X"));
                }
                Attributes.Add(Atts);
            }
        }

        private void ReadBuffers(DataReader r)
        {
            // Vertex Buffer
            r.Skip(4);
            uint size = r.ReadUInt32();

            uint AttributeStart = r.Position - 8 + size;

            int BufferCount = r.ReadInt32();

            if (size - 0xC > 0)
            {
                for (int c = 0; c < BufferCount; c++)
                {
                    int unk = r.ReadInt32();
                    int VertexSize = r.ReadInt32(); // Stride
                    int VertexCount = r.ReadInt32();
                    int Unk = r.ReadInt32();
                    Console.WriteLine("Buffer " + c + ": 0x" + r.Position.ToString("x") + " " + unk.ToString("X") + " " + Unk.ToString("X"));

                    var buffer = new G1MBuffer();
                    buffer.Stride = VertexSize;
                    buffer.Count = VertexCount;
                    buffer.Buffer = r.GetSection(r.Position, buffer.Stride * buffer.Count);
                    if (buffer.Buffer.Length / buffer.Stride != buffer.Count)
                        throw new Exception(r.Position.ToString("X"));
                    Buffers.Add(buffer);

                    r.Skip((uint)(VertexSize * VertexCount));
                }

            }
        }

        private void ReadMaterial(DataReader r)
        {
            r.PrintPosition();
            Console.WriteLine(r.ReadInt32()); // 9
            // Material Section
            {
                r.Skip(4);
                uint Size = r.ReadUInt32();
                r.Skip(Size - 8);
            }
            {
                r.Skip(4);
                uint Size = r.ReadUInt32();
                uint ss = r.Position + Size - 8;
                int Count = r.ReadInt32();

                for (int i = 0; i < Count; i++)
                {
                    G1TextureBank material = new G1TextureBank();
                    //3ds
                    if (type.Equals("3DS"))
                    {
                        r.Skip(0x10);
                        material.DiffuseTextureIndex = r.ReadInt32();
                        r.Skip(0x14);
                    }
                    else
                    //switch
                    {
                        r.PrintPosition();
                        r.Skip(0x4);
                        int texCount = r.ReadInt32();
                        r.Skip(8);
                        if(texCount > 0)
                        {
                            material.DiffuseTextureIndex = r.ReadInt16();
                            r.Skip((uint)(12 * texCount - 2));
                        }
                    }
                    TextureBanks.Add(material);
                    //Console.WriteLine("Mat" + Materials[i]);
                }
                r.Seek(ss);
            }
            {
                r.Skip(4);
                uint Size = r.ReadUInt32();
                uint ss = r.Position + Size - 8;

                int Count = r.ReadInt32();

                for (int i = 0; i < Count; i++)
                {
                    int paramCount = r.ReadInt32();

                    G1Material material = new G1Material();

                    //Console.WriteLine("Material " + i);

                    for(int j = 0; j < paramCount; j++)
                    {
                        uint length = r.ReadUInt32();
                        uint nextParam = r.Position - 4 + length;
                        int unkCount1 = r.ReadInt32();
                        int unkCount2 = r.ReadInt32();
                        int count = r.ReadInt16();
                        int type = r.ReadInt16();

                        string name = r.ReadString(r.Position, -1);
                        r.Skip((uint)name.Length);
                        r.Align(4);

                        //Console.WriteLine("\tParam " + name + " " + j + " " + type + " " + count);

                        object obj = null;

                        if (type == 1)
                        {
                            var val = new float[count];
                            for (int k = 0; k < count; k++)
                                val[k] = r.ReadSingle();
                            obj = val;
                        }
                        else
                            Console.WriteLine("Unknown Material Type: " + r.Position.ToString("X") + " " + type);

                        material.Parameters.Add(name, obj);

                        r.Seek(nextParam);
                    }

                    Materials.Add(material);
                }

                r.Seek(ss);
            }
        }

        public List<GenericVertex> GetVertices(G1MPolygon poly, out List<uint> indices, bool getWeights)
        {
            indices = new List<uint>();

            var buff = GetVertex(poly.Buffer);

            var indBuff = GetIndices(poly);

            Dictionary<ushort, uint> indexToIndex = new Dictionary<ushort, uint>();
            List<GenericVertex> vertices = new List<GenericVertex>();

            foreach (ushort i in indBuff)
            {
                if (indexToIndex.ContainsKey(i))
                {
                    indices.Add(indexToIndex[i]);
                    continue;
                }

                var vert = buff[i];
                if (vert.Weights == Vector4.Zero)
                {
                    vert.Bones = new Vector4(BindMatches[poly.BoneTableIndex][0], 0, 0, 0);
                    vert.Weights = new Vector4(1, 0, 0, 0);
                }
                else if (getWeights)
                {
                    //Console.WriteLine(BindMatches[poly.BoneTableIndex].Length + " " + vert.Bones.ToString() + " " + vert.Weights.ToString());
                    vert.Bones = new Vector4(BindMatches[poly.BoneTableIndex][(int)vert.Bones.X / 3],
                        BindMatches[poly.BoneTableIndex][(int)vert.Bones.Y / 3],
                        BindMatches[poly.BoneTableIndex][(int)vert.Bones.Z / 3],
                        BindMatches[poly.BoneTableIndex][(int)vert.Bones.W / 3]);
                }

                /*if (vert.Bones.X >= myStart - 1 && myStart != 0)
                {
                    //Console.WriteLine(myStart + " " + p.vBuffer[ib[j]].node.ToString());
                    p.vBuffer[ib[j]].pos = Vector3.Transform(p.vBuffer[ib[j]].pos,
                        Root.transform);
                }*/

                indexToIndex.Add(i, (uint)vertices.Count);
                indices.Add((uint)vertices.Count);
                vertices.Add(vert);
            }

            return vertices;

            //return vertices;
        }

        public List<uint> GetIndices(G1MPolygon poly)
        {
            var indices = IndexBuffers[poly.Buffer];

            List<uint> vertices = new List<uint>();

            for (int i = poly.FaceOffset; i < poly.FaceOffset + poly.FaceCount; i++)
            {
                vertices.Add(indices[i]);
            }

            return vertices;
        }

        public List<GenericVertex> GetVertex(int bufferIndex)
        {
            var buffer = Buffers[bufferIndex];
            var attributes = Attributes[bufferIndex];

            var max = 0;

            //Console.WriteLine(i + " " + buffer.Count + " " + buffer.Buffer.Length / buffer.Stride);

            List<GenericVertex> vertices = new List<GenericVertex>();

            using (DataReader r = new DataReader(new MemoryStream(buffer.Buffer)))
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    GenericVertex vert = new GenericVertex();
                    foreach (var att in attributes)
                    {
                        r.Seek((uint)(i * buffer.Stride + att.offset));

                        switch ((G1MVertexAttribute)att.semantic)
                        {

                            case G1MVertexAttribute.POSITION: //Position
                                if (att.datatype < 4)
                                {
                                    vert.Pos = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                    vert.Extra3.X = r.ReadSingle();
                                }
                                else
                                {
                                    throw new Exception("Verty 0x" + att.datatype.ToString("x"));
                                }
                                break;
                            case G1MVertexAttribute.WEIGHT: //Weights?
                                if (att.datatype == 0x0B)
                                {
                                    vert.Weights = new Vector4(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                }
                                else if (att.datatype == 0x02)
                                {
                                    vert.Weights = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 0); // 4th is calculated?
                                }
                                else if (att.datatype == 0x03)
                                {
                                    vert.Weights = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                }
                                else if (att.datatype == 0x0A)
                                {
                                    vert.Weights = new Vector4(r.ReadHalfSingle(),
                                        r.ReadHalfSingle(),
                                        0,
                                        0);
                                    vert.Weights.Z = 1.0f - vert.Weights.X - vert.Weights.Y;
                                }
                                else
                                {
                                    throw new Exception("Weight " + att.datatype);
                                    //Console.WriteLine();
                                }
                                //
                                break;
                            case G1MVertexAttribute.BONES: //Nodes?
                                if (att.datatype == 0x05)
                                {
                                    //if(!cloth)
                                    vert.Bones = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
                                    //Console.WriteLine((vertBuffer[i].node/3).ToString());
                                }
                                else
                                if (att.datatype == 0x0D)
                                {
                                    vert.Bones = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
                                }
                                else
                                {
                                    Console.WriteLine("Bone Weight " + att.datatype);
                                }
                                break;
                            case G1MVertexAttribute.NORMALS: //Normals?
                                if (att.datatype == 0x0b)
                                {
                                    vert.Nrm = new Vector3(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                    vert.Extra3.Y = (r.ReadHalfSingle());
                                }
                                if (att.datatype == 0x02 || att.datatype == 0x03)
                                {
                                    vert.Nrm = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                    vert.Extra3.Y = (r.ReadSingle());
                                }
                                break;
                            case G1MVertexAttribute.TEXCOORD0: //UV?
                                var uv = Vector2.Zero;
                                if (att.datatype == 0x0A || att.datatype == 1)
                                {
                                    uv = new Vector2(r.ReadHalfSingle(), r.ReadHalfSingle());
                                }
                                else
                                if (att.datatype == 0x5)
                                {
                                    vert.Extra2 = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
                                }
                                else
                                    Console.WriteLine(att.datatype);
                                if (att.track == 0)
                                    vert.UV0 = uv;
                                break;
                            case G1MVertexAttribute.BINORMAL:
                                if (att.datatype == 0x02)
                                {
                                    vert.Bit = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1);
                                }
                                else if (att.datatype == 0x03)
                                {
                                    vert.Bit = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                }
                                else if (att.datatype == 0x0B)
                                {
                                    vert.Bit = new Vector4(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                }
                                else
                                    throw new Exception("Unknown vertex stream 0x0A01 type.");
                                break;
                            case G1MVertexAttribute.TANGENT:
                                if (att.datatype == 0x02)
                                {
                                    vert.Tan = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1);
                                }
                                else if (att.datatype == 0x03)
                                {
                                    vert.Tan = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                }
                                else if (att.datatype == 0x0B)
                                {
                                    vert.Tan = new Vector4(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                }
                                else
                                    throw new Exception("Unknown vertex stream 0x0600 type.");
                                break;
                            case G1MVertexAttribute.FOG:
                                if (att.track == 0 && att.datatype == 5)
                                {
                                    vert.Fog = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
                                }
                                break;
                            case G1MVertexAttribute.COLOR:
                                var colr = Vector4.One;
                                if (att.datatype == 0x02)
                                    colr = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), 1);
                                else if (att.datatype == 0x03)
                                    colr = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                                else if (att.datatype == 0x0B)
                                    colr = new Vector4(r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle(), r.ReadHalfSingle());
                                else if (att.datatype == 0x0D)
                                    colr = new Vector4(r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f, r.ReadByte() / 255f);
                                else
                                    throw new Exception("Unknown vertex stream 0x0A01 type.");

                                if (att.track == 0)
                                    vert.Clr = Vector4.One;// colr;
                                else
                                    vert.Clr1 = colr;
                                break;
                            case G1MVertexAttribute.UNK1:
                                if (att.datatype == 0x05)
                                {
                                    vert.Extra = new Vector4(r.ReadByte(), r.ReadByte(), r.ReadByte(), r.ReadByte());
                                }
                                break;
                            default:
                                //if(type.Equals("3DS"))
                                //File.WriteAllBytes("dump.bin", buffer.Buffer);
                                throw new Exception("Unknown Semantic " + att.semantic.ToString("x") + " " + att.offset.ToString("X"));
                                //break;
                        }
                    }

                    vertices.Add(vert);

                }
            }
            Console.WriteLine("Max: " + max.ToString("X"));

            return vertices;
        }
    }

}
