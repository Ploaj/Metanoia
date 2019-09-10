using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metanoia.Modeling;

namespace Metanoia.Formats.PS2
{
    [Format(".fpk")]
    public class FPK : IModelContainerFormat
    {
        private GenericModel Model;

        public string Name => "FPK";
        public string Extension => ".fpk";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        public List<FileItem> Item = new List<FileItem>();

        public FileItem[] GetFiles()
        {
            return Item.ToArray();
        }

        public void Open(FileItem File)
        {
            Model = new GenericModel();

            using (DataReader r = new DataReader(File))
            {
                r.Seek(4);
                var count = r.ReadInt32();

                r.Seek(0x10);
                for(int i =0; i < count; i++)
                {
                    var name = r.ReadString(r.Position, -1);
                    r.Skip(0x24);
                    var offset = r.ReadUInt32();
                    var compSize = r.ReadInt32();
                    var decompSize = r.ReadInt32();

                    var item = new FileItem(name, Tools.Decompress.PRS_Mod(r.GetSection(offset, compSize), decompSize, compSize));

                    Item.Add(item);
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public GenericModel ToGenericModel()
        {
            return Model;
        }

        public bool Verify(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.Seek(0xC);
                if (r.ReadUInt32() == file.Length)
                    return true;
            }
            return false;
        }
    }
}
