using System.Windows.Forms;
using System.IO;

namespace Metanoia.GUI
{
    public class FileItem
    {
        public string Text;
        public string FilePath;

        public string Extension { get { return Path.GetExtension(FilePath); } }

        public FileItem(string FilePath)
        {
            Text = Path.GetFileName(FilePath);
            this.FilePath = FilePath;
        }

        public override string ToString()
        {
            return Text;
        }

        public byte[] GetFileBinary()
        {
            return File.ReadAllBytes(FilePath);
        }
    }
}
