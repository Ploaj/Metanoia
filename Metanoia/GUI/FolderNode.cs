using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace Metanoia.GUI
{
    public class FolderNode : TreeNode
    {
        public string FilePath
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                Text = Path.GetFileName(_path.Replace(":", ""));
            }
        }
        private string _path;

        public FolderNode(string FilePath)
        {
            this.FilePath = FilePath;
            AfterCollapse();
        }

        public List<FileItem> GetFiles()
        {
            List<FileItem> Items = new List<FileItem>();

            foreach(string s in Directory.GetFiles(_path))
            {
                if (!File.GetAttributes(s).HasFlag(FileAttributes.Directory))
                {
                    Items.Add(new FileItem(s));
                }
            }

            return Items;
        }

        public void ExpandNode(string FilePath)
        {
            string Name = Path.GetFileName(FilePath);

            foreach (TreeNode n in Nodes)
            {
                if (n.Text.Equals(Name))
                {
                    n.Expand();
                    break;
                }
            }
        }
        
        public void BeforeExpand()
        {
            if (IsExpanded) return;
            Nodes.Clear();
            foreach(string s in Directory.GetDirectories(_path + "\\"))
            {
                if(File.GetAttributes(s).HasFlag(FileAttributes.Directory))
                    Nodes.Add(new FolderNode(s));
            }
        }

        public void AfterCollapse()
        {
            Nodes.Clear();
            Nodes.Add(new TreeNode("Dummy"));
        }
    }
}
