using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.Misc
{
    // incomplete
    public class NHMDL 
    {
        public string Name => "";

        public string Extension => ".nhmdl";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                var metaOffset = r.Position + r.ReadUInt64();
                var metaLength = r.ReadInt32();

                r.Seek((uint)metaOffset);
            }
        }

        private void ReadMetaData(DataReader r)
        {
            var boneDataOffset = r.Position + r.ReadUInt64();
            var boneDataCount = r.ReadInt64();

            r.Position = (uint)boneDataOffset;
            for(var i = 0; i < boneDataCount; i++)
            {
                var boneNameOffset = r.Position + r.ReadUInt64();
                var boneNameLength = r.ReadInt64();
                var parentIndex = r.ReadInt32();
                r.ReadInt32();

            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            throw new NotImplementedException();
        }

        public bool Verify(FileItem file)
        {
            return file.Extension.ToLower() == Extension;
        }
    }
}
