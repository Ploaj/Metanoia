using Metanoia.Formats;
using Metanoia.GUI;
using System;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Metanoia
{
    public partial class ExploreForm : DockContent
    {
        public static ExploreForm Instance;

        public ExploreForm()
        {
            InitializeComponent();

            IsMdiContainer = true;
        }

        public void AddDockedControl(DockContent content)
        {
            if (content != null && dockPanel != null)
                content.Show(dockPanel);
        }

        private void ExploreForm_DragDrop(object sender, DragEventArgs e)
        {
            foreach(var f in e.Data.GetData(DataFormats.FileDrop) as string[])
            {
                OpenFile(f);
            }
        }

        private void OpenFile(string filePath)
        {
            var file = FormatManager.Instance.Open(new FileItem(filePath));
            if (file != null)
            {
                if (file is IContainerFormat container)
                {
                    ExploreContainer explore = new ExploreContainer(container);
                    explore.Text = Path.GetFileName(filePath);
                    AddDockedControl(explore);
                }
                else
                if (file is I3DModelFormat model)
                {
                    ModelExplore explore = new ModelExplore(model);
                    explore.Text = Path.GetFileName(filePath);
                    AddDockedControl(explore);
                }
            }
        }

        private void ExploreForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Filter = FormatManager.Instance.GetExtensionFilter();

                if(d.ShowDialog() == DialogResult.OK)
                {
                    OpenFile(d.FileName);
                }
            }
        }
    }
}
