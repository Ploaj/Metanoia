using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MINF
    {
        public uint AnimationName;

        public uint AnimationSubName;

        public int FrameStart;

        public int FrameEnd;

        public void Open(byte[] data)
        {
            using (DataReader reader = new DataReader(data))
            {
                reader.BigEndian = false;

                reader.Seek(0x1C);
                AnimationSubName = reader.ReadUInt32();

                reader.Seek(0x44);
                AnimationName = reader.ReadUInt32();

                reader.Seek(0x4C);
                FrameStart = reader.ReadInt32();
                FrameEnd = reader.ReadInt32();
            }
        }
    }
}
