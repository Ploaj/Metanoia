using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;
using Metanoia.Tools;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_MTN2 : IAnimationFormat
    {
        public string Name => "Level5 Motion";
        public string Extension => ".mtn2";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        private GenericAnimation anim = new GenericAnimation();

        public void Open(FileItem file)
        {
            anim.Name = file.FilePath;

            using(DataReader r = new DataReader(file))
            {
                r.BigEndian = false;

                r.Seek(0x08);
                var decomSize = r.ReadInt32();
                var nameOffset = r.ReadUInt32();
                var compDataOffset = r.ReadUInt32();
                var floatDataCount = r.ReadInt32();
                var shortDataCount = r.ReadInt32();
                var extraDataCount1 = r.ReadInt32();
                var extraDataCount2 = r.ReadInt32();
                var boneCount = r.ReadInt32();

                r.Seek(0x54);
                anim.FrameCount = r.ReadInt32();

                r.Seek(nameOffset);
                var hash = r.ReadUInt32();
                anim.Name = r.ReadString(r.Position, -1);
                
                var data = Decompress.Level5Decom(r.GetSection(compDataOffset, (int)(r.Length - compDataOffset)));
                System.IO.File.WriteAllBytes(file.FilePath.Replace(".mtn2", ".bin"), data);
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericAnimation ToGenericAnimation()
        {
            return anim;
        }

        public bool Verify(FileItem file)
        {
            return (file.MagicString == "XMTN");
        }
    }
}
