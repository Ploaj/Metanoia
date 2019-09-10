using OpenTK;
using System;

namespace Metanoia.Formats.Misc
{
    public class NUNV : NUNO
    {
        public NUNV(DataReader r)
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
                        Read_00030005(r); //Originally 5
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
            r.Skip(0x5C); //Roginally 68
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

            r.Skip(0x5C);

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
