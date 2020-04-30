using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats.Misc
{
    /// <summary>
    ///  Only Gamecube Version Supporte
    /// </summary>
    public class F9Res : IContainerFormat
    {
        public string Name => "Re";
        public string Extension => ".res";
        public string Description => "";
        public bool CanOpen => true;
        public bool CanSave => false;

        public List<FileItem> Files = new List<FileItem>();

        public FileItem[] GetFiles()
        {
            return Files.ToArray();
        }

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = true;

                r.Seek(0x08);
                var headerOffset = r.ReadUInt32();

                r.Seek(0x1C);
                var chunksOffset = r.ReadUInt32();

                r.Seek(chunksOffset);
                var chunkCount = r.ReadInt32();
                r.ReadInt32(); //4
                var tableOffset = r.Position + 20 * chunkCount;
                for(int i = 0; i < chunkCount; i++)
                {
                    var name = new string(r.ReadChars(4));
                    var offset = r.ReadUInt32();
                    var length = r.ReadInt32();
                    var unknown = r.ReadInt32();
                    var unknown2 = r.ReadInt32();

                    var temp = r.Position;
                    r.Seek((uint)tableOffset);
                    Console.WriteLine(name);
                    for(int j = 0; j < unknown; j++)
                    {
                        Console.WriteLine("\t" + r.ReadInt32().ToString("X"));
                    }
                    tableOffset += unknown * 4;
                    r.Seek(temp);

                    FileItem item = new FileItem(name, r.GetSection(offset + headerOffset, length));

                    Files.Add(item);
                }
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool Verify(FileItem file)
        {
            return file.Magic == 0x7265730A;
        }
        
    }
}
