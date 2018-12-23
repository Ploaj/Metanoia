using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Metanoia.Formats.N64
{
    public struct N64Vertex_Normal
    {
        public Vector3 pos;
        public int bone;
        public Vector2 uv;
        public Vector4 color;
        public const int Stride = 4 * (3 + 2 + 4 + 1);
    }

    public class N64DisplayList
    {
        public List<N64Vertex_Normal> verts = new List<N64Vertex_Normal>();
        public List<int> faces = new List<int>();
    }

    public class N64Tools
    {
        private static N64Vertex_Normal[] vertBuffer = new N64Vertex_Normal[32];

        public static N64DisplayList ReadDisplayList(DataReader reader, int BoneIndex, Matrix4 Transform)
        {
            int vbidindex = 0;
            int vertIndex = 0;
            int size = 0;
            bool done = false;
            N64DisplayList list = new N64DisplayList();
            int f1 = 0, f2 = 0, f3 = 0;
            while (!done)
            {
                //Console.WriteLine(reader.pos().ToString("x"));
                int key = reader.ReadByte();

                switch (key)
                {
                    case 0x01: // G_VTX
                        // just to offset
                        vertIndex = list.verts.Count;
                        size = reader.ReadInt16() >> 4;
                        vbidindex = (reader.ReadByte() >> 1) - size;
                        int offset = reader.ReadInt32() & 0x1FFFF;

                        uint temp = reader.Position();
                        reader.Seek((uint)offset);
                        for (int v = 0; v < size; v++)
                        {
                            vertBuffer[vbidindex + v] = new N64Vertex_Normal()
                            {
                                pos = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()),// Vector3.Transform(, b == null ? Matrix4.CreateTranslation(0,0,0) : b.transform),
                                bone = reader.ReadInt16(),
                                uv = new Vector2((float)S10p5(reader.ReadInt16()), (float)S10p5(reader.ReadInt16())) / 32f,
                                color = new Vector4((sbyte)reader.ReadByte(), (sbyte)reader.ReadByte(), (sbyte)reader.ReadByte(), reader.ReadByte()) / sbyte.MaxValue
                            };
                            if (Transform != null)
                            {
                                vertBuffer[vbidindex + v].pos = Vector3.TransformPosition(vertBuffer[vbidindex + v].pos, Transform);
                                //if (!Transform.Inverted().Equals(Transform))
                                    vertBuffer[vbidindex + v].color.Xyz = Vector3.TransformNormal(vertBuffer[vbidindex + v].color.Xyz, Transform);
                            }
                            vertBuffer[vbidindex + v].bone = BoneIndex;
                        }
                        reader.Seek(temp);

                        break;
                    case 0x05: // G_TRI1
                        f1 = reader.ReadByte() / 2;
                        f2 = reader.ReadByte() / 2;
                        f3 = reader.ReadByte() / 2;
                        reader.Skip(4);

                        vertBuffer[f1].bone = BoneIndex;
                        vertBuffer[f2].bone = BoneIndex;
                        vertBuffer[f3].bone = BoneIndex;

                        if (!list.verts.Contains(vertBuffer[f1])) list.verts.Add(vertBuffer[f1]);
                        if (!list.verts.Contains(vertBuffer[f2])) list.verts.Add(vertBuffer[f2]);
                        if (!list.verts.Contains(vertBuffer[f3])) list.verts.Add(vertBuffer[f3]);
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f1]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f2]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f3]));
                        break;
                    case 0x06: // G_TRI2
                        f1 = reader.ReadByte() / 2;
                        f2 = reader.ReadByte() / 2;
                        f3 = reader.ReadByte() / 2;
                        vertBuffer[f1].bone = BoneIndex;
                        vertBuffer[f2].bone = BoneIndex;
                        vertBuffer[f3].bone = BoneIndex;
                        if (!list.verts.Contains(vertBuffer[f1])) list.verts.Add(vertBuffer[f1]);
                        if (!list.verts.Contains(vertBuffer[f2])) list.verts.Add(vertBuffer[f2]);
                        if (!list.verts.Contains(vertBuffer[f3])) list.verts.Add(vertBuffer[f3]);
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f1]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f2]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f3]));
                        reader.Skip(1);
                        f1 = reader.ReadByte() / 2;
                        f2 = reader.ReadByte() / 2;
                        f3 = reader.ReadByte() / 2;
                        vertBuffer[f1].bone = BoneIndex;
                        vertBuffer[f2].bone = BoneIndex;
                        vertBuffer[f3].bone = BoneIndex;
                        if (!list.verts.Contains(vertBuffer[f1])) list.verts.Add(vertBuffer[f1]);
                        if (!list.verts.Contains(vertBuffer[f2])) list.verts.Add(vertBuffer[f2]);
                        if (!list.verts.Contains(vertBuffer[f3])) list.verts.Add(vertBuffer[f3]);
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f1]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f2]));
                        list.faces.Add(list.verts.IndexOf(vertBuffer[f3]));
                        break;
                    case 0xD9: // G_GEOMETRYMODE
                        reader.Skip(7);
                        break;
                    case 0xDF: // G_ENDDL
                        reader.Skip(7);
                        done = true;
                        break;
                    default:
                        throw new Exception("Unknown Command " + key.ToString("x"));
                }
            }
            return list;
        }

        public static double S10p5(int p)
        {
            double m = 0;

            int n = p & 0x1F;
            for (int i = 0; i < 5; i++)
            {
                if ((n & 0x1) == 1)
                {
                    m += (double)Math.Pow(2, -(5 - i));
                }
                n = n >> 1;
            }

            m += Sign10thBit((p >> 5));

            return m;
        }

        public static int Sign10thBit(int i)
        {
            if (((i >> 9) & 0x1) == 1)
            {
                i = ~i;
                i = i & 0x3FF; // 3 7
                i += 1;
                i *= -1;
            }

            return i;
        }
    }
}
