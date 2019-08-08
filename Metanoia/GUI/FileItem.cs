using System.Windows.Forms;
using System.IO;

namespace Metanoia
{
    public class FileItem : ListViewItem
    {
        public string FilePath { get; set; }

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
