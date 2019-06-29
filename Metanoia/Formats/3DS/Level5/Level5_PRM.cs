using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using Metanoia.Tools;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_PRM
    {
        public string Name;
        
        public byte[] PolygonVertexBuffer;
        public byte[] PolygonVertexIndexBuffer;

        public String MaterialName;

        private float[] NodeTable;

        public Level5_PRM(byte[] data)
        {
            using (DataReader r = new DataReader(new System.IO.MemoryStream(data)))
            {
                Open(r);
            }
        }

        public void Open(DataReader r)
        {
            r.Seek(4);
            var prmOffset = r.ReadUInt32();

            r.Seek(prmOffset);
            r.Skip(4);

            // buffers-------------------------------------------

            uint pvbOffset = r.ReadUInt32();
            int pvbSize = r.ReadInt32();
            uint pviOffset = r.ReadUInt32();
            int pviSize = r.ReadInt32();

            PolygonVertexBuffer = r.GetSection(pvbOffset + prmOffset, pvbSize);
            PolygonVertexIndexBuffer = r.GetSection(pviOffset + prmOffset, pviSize);

            // node table-------------------------------------------

            r.Seek(0x28);
            uint noOffset = r.ReadUInt32();
            int noSize = r.ReadInt32() / 4 + 1;

            NodeTable = new float[noSize];
            r.Seek(noOffset);
            for (int i = 0; i < noSize; i++)
            {
                NodeTable[i] = r.ReadSingle();
            }

            // name and material-------------------------------------------
            r.Seek(0x30);
            string name = r.ReadString(r.ReadUInt32(), r.ReadInt32());
            MaterialName = r.ReadString(r.ReadUInt32(), r.ReadInt32());
            Name = name;
        }

        public static GenericMesh ToGenericMesh(byte[] data)
        {
            Level5_PRM prm = new Level5_PRM(data);
            return prm.ToGenericMesh();
        }

        public GenericMesh ToGenericMesh()
        {
            GenericMesh mesh = new GenericMesh();

            mesh.Name = Name;
            mesh.MaterialName = MaterialName;

            mesh.Triangles = ParseIndexBuffer(PolygonVertexIndexBuffer);
            mesh.Vertices = ParseBuffer(PolygonVertexBuffer);

            return mesh;
        }
        
        private List<uint> ParseIndexBuffer(byte[] buffer)
        {
            List<uint> Indices = new List<uint>();
            int FaceCount = 0;
            using (DataReader r = new DataReader(new System.IO.MemoryStream(buffer)))
            {
                r.Seek(0x06);
                uint faceOffset = r.ReadUInt16();
                FaceCount = r.ReadInt32();

                buffer = Decompress.Level5Decom(r.GetSection(faceOffset, (int)(r.Length - faceOffset)));
            }

            using (DataReader r = new DataReader(new System.IO.MemoryStream(buffer)))
            {
                r.Seek(0);
                int f1 = r.ReadInt16();
                int f2 = r.ReadInt16();
                int f3;
                int dir = -1;
                int startdir = -1;
                for (int i = 0; i < FaceCount; i++)
                {
                    if (r.Position + 2 > r.Length)
                        break;
                    f3 = r.ReadInt16();
                    if (f3 == 0xFFFF || f1 == -1)
                    {
                        f1 = r.ReadInt16();
                        f2 = r.ReadInt16();
                        dir = startdir;
                    }
                    else
                    {
                        dir *= -1;
                        if (f1 != f2 && f2 != f3 && f3 != f1)
                        {
                            /*if (f1 > vCount || f2 > vCount || f3 > vCount)
                            {
                                f1 = 0;
                            }*/
                            if (dir > 0)
                            {
                                Indices.Add((uint)f1);
                                Indices.Add((uint)f2);
                                Indices.Add((uint)f3);
                            }
                            else
                            {
                                Indices.Add((uint)f1);
                                Indices.Add((uint)f3);
                                Indices.Add((uint)f2);
                            }
                        }
                        f1 = f2;
                        f2 = f3;
                    }
                }
            }
            return Indices;
        }

        private List<GenericVertex> ParseBuffer(byte[] buffer)
        {
            List<GenericVertex> Vertices = new List<GenericVertex>();
            byte[] attributeBuffer = new byte[0];
            int stride = 0;
            int vertexCount = 0;
            using (DataReader r = new DataReader(new System.IO.MemoryStream(buffer)))
            {
                r.Seek(0x4);
                uint attOffset = r.ReadUInt16();
                int attSomething = r.ReadInt16();
                uint verOffset = r.ReadUInt16();
                stride = r.ReadInt16();
                vertexCount = r.ReadInt32();

                attributeBuffer = Decompress.Level5Decom(r.GetSection(attOffset, attSomething));
                buffer = Decompress.Level5Decom(r.GetSection(verOffset, (int)(r.Length - verOffset)));
            }

            int[] ACount = new int[10];
            int[] AOffet = new int[10];
            int[] ASize = new int[10];
            int[] AType = new int[10];
            using (DataReader r = new DataReader(new System.IO.MemoryStream(attributeBuffer)))
            {
                for (int i = 0; i < 10; i++)
                {
                    ACount[i] = r.ReadByte();
                    AOffet[i] = r.ReadByte();
                    ASize[i] = r.ReadByte();
                    AType[i] = r.ReadByte();

                    if (ACount[i] > 0 && i != 0 && i != 1 && i != 2 && i != 4 && i != 7 && i != 8 && i != 9)
                    {
                        Console.WriteLine(i + " " + ACount[i] + " " + AOffet[i] + " " + ASize[i] + " " + AType[i]);
                    }
                }
            }

            using (DataReader r = new DataReader(new System.IO.MemoryStream(buffer)))
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    GenericVertex vert = new GenericVertex();
                    vert.Clr = new Vector4(1, 1, 1, 1);
                    for (int j = 0; j < 10; j++)
                    {
                        r.Seek((uint)(i * stride + AOffet[j]));
                        switch (j)
                        {
                            case 0: //Position
                                vert.Pos = ReadAttribute(r, AType[j], ACount[j]).Xyz;
                                break;
                            case 1: //Tangent
                                break;
                            case 2: //Normal
                                vert.Nrm = ReadAttribute(r, AType[j], ACount[j]).Xyz;
                                break;
                            case 4: //UV0
                                vert.UV0 = ReadAttribute(r, AType[j], ACount[j]).Xy;
                                break;
                            case 7: //Bone Weight
                                vert.Weights = ReadAttribute(r, AType[j], ACount[j]);
                                break;
                            case 8: //Bone Index
                                Vector4 vn = ReadAttribute(r, AType[j], ACount[j]);
                                if (NodeTable.Length > 0 && NodeTable.Length != 1)
                                    vert.Bones = new Vector4(NodeTable[(int)vn.X], NodeTable[(int)vn.Y], NodeTable[(int)vn.Z], NodeTable[(int)vn.W]);
                                break;
                            case 9: // not sure
                                //vert.Clr = ReadAttribute(r, AType[j], ACount[j]).Wxyz;
                                break;
                        }
                    }
                    Vertices.Add(vert);
                }
            }

            //
            return Vertices;
        }

        public Vector4 ReadAttribute(DataReader f, int type, int count)
        {
            Vector4 o = new Vector4(1);
            switch (type)
            {
                case 0://nothing

                    break;
                case 1:
                    if (count > 0 && f.Position + 4 < f.Length)
                        o.X = f.ReadSingle();
                    if (count > 1 && f.Position + 4 < f.Length)
                        o.Y = f.ReadSingle();
                    if (count > 2 && f.Position + 4 < f.Length)
                        o.Z = f.ReadSingle();
                    break;
                case 2: //Float
                    if (count > 0 && f.Position + 4 < f.Length)
                        o.X = f.ReadSingle();
                    if (count > 1 && f.Position + 4 < f.Length)
                        o.Y = f.ReadSingle();
                    if (count > 2 && f.Position + 4 < f.Length)
                        o.Z = f.ReadSingle();
                    if (count > 3 && f.Position + 4 < f.Length)
                        o.W = f.ReadSingle();
                    break;
                default:
                    throw new Exception("Unknown Type 0x" + type.ToString("x") + " " + f.ReadInt32().ToString("X") + f.ReadInt32().ToString("X"));
            }
            return o;
        }
    }
}