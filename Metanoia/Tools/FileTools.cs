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
        /// <param name="defaultPath"></param>
        /// <returns></returns>
        public static string GetFolder(string defaultPath = "")
        {
            using (FolderBrowserDialog f = new FolderBrowserDialog())
            {
                f.SelectedPath = defaultPath;

                if (f.ShowDialog() == DialogResult.OK)
                    return f.SelectedPath;
            }

            return null;
        }

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
                    File.WriteAllBytes(d.FileName, item.GetFileBinary());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string GetSaveFile(string defaultName = "", string filter = "")
        {
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.Filter = filter;
                d.FileName = defaultName;

                if (d.ShowDialog() == DialogResult.OK)
                {
                    return d.FileName;
                }
            }
            return null;
        }

    }
}
