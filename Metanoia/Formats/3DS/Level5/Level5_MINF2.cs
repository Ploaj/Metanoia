using System;
using System.Text;
using System.Collections.Generic;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MINF2
    {
        public List<SubAnimation> SubAnimations;

        public class SubAnimation
        {
            public uint AnimationName;

            public string AnimationSubName;

            public int FrameStart;

            public int FrameEnd;
        }

        public void Open(byte[] data)
        {
            using (DataReader reader = new DataReader(data))
            {
                reader.BigEndian = false;

                List<int> offsets = new List<int>();

                while (true)
                {
                    string tryName = reader.ReadString(4);

                    if (tryName == "MINF")
                    {
                        reader.Skip(0x04);
                        int minfDataOffset = reader.ReadInt32();
                        offsets.Add(minfDataOffset);
                        reader.Skip(0x0C);
                    } else
                    {
                        reader.Seek(reader.Position - 4);
                        break;
                    }                  
                }

                SubAnimations = new List<SubAnimation>();

                Console.WriteLine("minf2pose = " + reader.Position);

                using (DataReader minfDataReader = new DataReader(reader.GetSection(reader.Position, (int)(reader.Length - reader.Position))))
                {
                    for (int i = 0; i < offsets.Count; i++)
                    {
                        minfDataReader.Seek((uint)offsets[i]);
                        uint hash = minfDataReader.ReadUInt32();
                        string name = minfDataReader.ReadString(Encoding.GetEncoding("Shift-JIS"));

                        minfDataReader.Seek((uint)offsets[i] + 0x28);

                        SubAnimation newSubAnimation = new SubAnimation();
                        newSubAnimation.AnimationName = minfDataReader.ReadUInt32();
                        minfDataReader.Skip(0x04);
                        newSubAnimation.AnimationSubName = name;
                        newSubAnimation.FrameStart = minfDataReader.ReadInt32();
                        newSubAnimation.FrameEnd = minfDataReader.ReadInt32();

                        SubAnimations.Add(newSubAnimation);
                    }
                }
            }
        }
    }
}
