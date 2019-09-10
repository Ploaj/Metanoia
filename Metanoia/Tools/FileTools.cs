using System;
using System.IO;
using System.Windows.Forms;

namespace Metanoia
{
    public class FileTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="defaultName"></param>
        /// <param name="filter"></param>
        public static void SaveFile(FileItem item, string defaultName = "", string filter = "")
        {
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.Filter = filter;
                d.FileName = Path.GetFileName(item.FilePath);
                if(filter == "")
                {
                    filter += $"({item.Extension})|*{item.Extension}";
                }

                if(d.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllBytes(d.FileName, item.GetFileBinary());
                }
            }
        }

    }
}
