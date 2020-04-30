using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Formats.GameCube
{
    public class aaaaZZZZ : IContainerFormat
    {
        public string Name => "";

        public string Extension => ".dat";

        public string Description => "";

        public bool CanOpen => true;

        public bool CanSave => false;

        public FileItem[] GetFiles()
        {
            return new FileItem[0];
        }

        public void Open(FileItem file)
        {
            using (DataReader r = new DataReader(file))
            {
                r.BigEndian = true;

                r.ReadInt32();
                var count = r.ReadInt32();

                List<int> values = new List<int>();
                for(int i = 0; i < count; i++)
                {
                    r.ReadInt32(); // flags
                    Console.WriteLine(r.ReadInt32().ToString("X") + " " + r.ReadInt32().ToString("X") + " " + r.ReadInt32().ToString("X"));
                }
                values.Sort();
                Console.WriteLine(string.Join("\n", values));
            }
        }

        public void Save(string filePath)
        {
            throw new NotImplementedException();
        }

        public bool Verify(FileItem file)
        {
            return file.MagicString.Equals("ZTAB");
        }
    }
}
