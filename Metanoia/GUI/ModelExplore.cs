using Metanoia.Formats;
using Metanoia.Rendering;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Metanoia.GUI
{
    public partial class ModelExplore : DockContent
    {
        private ModelViewer ModelViewer = new ModelViewer();

        private I3DModelFormat Model;

        public ModelExplore(I3DModelFormat model)
        {
            InitializeComponent();

            ModelViewer.Dock = DockStyle.Fill;
            viewerBox.Controls.Add(ModelViewer);

            Model = model;
            ModelViewer.SetModel(model.ToGenericModel());
        }

        ~ModelExplore()
        {
            ModelViewer.Dispose();
        }
    }
}
