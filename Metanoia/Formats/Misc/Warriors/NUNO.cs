using OpenTK;
using System;
using System.Collections.Generic;

namespace Metanoia.Formats.Misc
{
    /// <summary>
    /// 
    /// </summary>
    public class NUNO
    {
        public struct NUNOInfluence
        {
            public int P1;
            public int P2;
            public int P3;
            public int P4;
            public float P5;
            public float P6;
        }

        public class NUNOv33Entry
        {
            public List<Vector4> Points = new List<Vector4>();
            public NUNOInfluence[] Influences;
            public int ParentBoneID;
            public int UnknownIndex;
        }

        public class NUNOv32Entry
        {
            public int ParentBone;
            public Vector3 Point;
        }

        public List<NUNOv32Entry> V32Entries = new List<NUNOv32Entry>();
        public List<NUNOv33Entry> V33Entries = new List<NUNOv33Entry>();
        public List<NUNOv33Entry> V35Entries = new List<NUNOv33Entry>();

        public NUNO()
        {

        }

        public NUNO(DataReader r)
        {
            int sectionCount = r.ReadInt32();

            for (int i = 0; i < sectionCount; i++)
            {
                var flags = r.ReadInt32();
                var end = r.Position + r.ReadUInt32() - 4;
                Console.WriteLine("Cloth Type: " + flags.ToString("X"));

                int entryCount = r.ReadInt32();

                for (int j = 0; j < entryCount; j++)
                {
                    r.PrintPosition();
                    if (flags == 0x00050001)
                    {
                        Read_00030005(r);
                    }
                    if (flags == 0x00030003)
                    {
                        Read_00030003(r);
                    }
                    if (flags == 0x00030002)
                    {
                        Read_00030002(r);
                    }
                }

                r.Seek(end);
            }
        }

        private void Read_00030002(DataReader r)
        {
            NUNOv32Entry entry = new NUNOv32Entry();
            V32Entries.Add(entry);

            entry.ParentBone = r.ReadInt32();
            r.Skip(0x68);
            entry.Point = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            r.Skip(8);
        }

        private void Read_00030003(DataReader r)
        {
            NUNOv33Entry entry = new NUNOv33Entry();
            V33Entries.Add(entry);

            entry.UnknownIndex = r.ReadInt32();
            int pointCount = r.ReadInt32();
            uint section2Count = r.ReadUInt32();
            var unk = r.ReadInt32();// r.Skip(4); // usually 1?
            entry.ParentBoneID = r.ReadInt32();
            var unk2 = r.ReadInt32();

            r.Skip(0xC0);

            for (int i = 0; i < pointCount; i++)
            {
                entry.Points.Add(new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }

            entry.Influences = r.ReadStructArray<NUNOInfluence>(pointCount);

            r.Skip(48 * section2Count);

            Console.WriteLine("rr " + r.Position.ToString("X") + " " + unk + " " + unk2);
            
            r.Skip((uint)unk * 4);
            r.Skip((uint)unk2 * 4 * 2);
            
        }


        private void Read_00030005(DataReader r)
        {
            NUNOv33Entry entry = new NUNOv33Entry();
            V33Entries.Add(entry);

            entry.UnknownIndex = r.ReadInt32();
            int pointCount = r.ReadInt32();
            uint section2Count = r.ReadUInt32();
            var unk = r.ReadInt32();// r.Skip(4); // usually 1?
            entry.ParentBoneID = r.ReadInt32();

            r.Skip(0x60);

            for (int i = 0; i < pointCount; i++)
            {
                entry.Points.Add(new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
            }

            entry.Influences = r.ReadStructArray<NUNOInfluence>(pointCount);

            r.Skip(48 * section2Count);

            Console.WriteLine("rr " + r.Position.ToString("X") + " " + unk);

            r.Skip((uint)unk * 4);

        }
    }
}
