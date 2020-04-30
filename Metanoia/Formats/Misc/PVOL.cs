using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.Misc
{
    public class PVOL : IContainerFormat
    {
        public string Name => "";

        public string Extension => ".pvol";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        private List<FileItem> Files = new List<FileItem>();

        public FileItem[] GetFiles()
        {
            return Files.ToArray();
        }

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = false;

                var fCount = r.ReadInt32();

                for(int i = 0; i < fCount - 1; i++)
                {
                    var off = r.ReadUInt32();
                    var length = r.ReadInt32();
                    
                    Files.Add(new FileItem(r.ReadString(off, -1) + r.ReadString(off + 0x20, -1), r.GetSection(off + 0x28, length)));
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return new GenericModel() { Skeleton = new GenericSkeleton()};
        }

        public bool Verify(FileItem file)
        {
            // todo: better verify is to check the length which is at the end of the table
            return file.Extension.ToLower() == Extension;
        }
    }
}
