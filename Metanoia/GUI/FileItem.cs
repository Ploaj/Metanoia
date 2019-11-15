using System.Windows.Forms;
using System.IO;

namespace Metanoia
{
    public class FileItem
    {
        public string FileName { get => Path.GetFileName(FilePath); }

        public string FilePath {
            get => _filePath;
            internal set
            {
                _filePath = value;
                Extension = Path.GetExtension(FilePath).ToLower();
                Length = new FileInfo(FilePath).Length;
            }
        }
        private string _filePath;

        public string MagicString
        {
            get
            {
                using (DataReader r = new DataReader(this))
                {
                    if (r.Length < 4)
                        return "";
                    return r.ReadString(4);
                }
            }
        }


        public uint Magic
        {
            get
            {
                using (DataReader r = new DataReader(this))
                {
                    r.BigEndian = true;
                    if (r.Length < 4)
                        return 0;
                    return r.ReadUInt32();
                }
            }
        }

        public string Extension { get; internal set; }

        public long Length { get; internal set; }
        
        private byte[] Data
        {
            get
            {
                if (EmbeddedFile)
                    return _data;
                else
                    return File.ReadAllBytes(FilePath);
            }
            set
            {
                EmbeddedFile = true;
                Length = value.Length;
                _data = value;
            }
        }
        private byte[] _data;

        public bool EmbeddedFile = false;

        public FileItem(string filePath, byte[] data)
        {
            _filePath = filePath;
            Extension = Path.GetExtension(filePath).ToLower(); ;
            Data = data;
        }

        public FileItem(string FilePath)
        {
            //Text = Path.GetFileName(FilePath);
            this.FilePath = FilePath;
        }

        public override string ToString()
        {
            return FilePath;
        }

        public byte[] GetFileBinary()
        {
            return Data;
        }
    }
}
