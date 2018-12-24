using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Rendering
{
    public partial class ModelViewer : UserControl
    {
        public Matrix4 Camera;

        public GenericRenderer GenericRenderer;

        public ModelViewer()
        {
            InitializeComponent();
        }

        public void RefreshRender()
        {
            Viewport.Invalidate();
        }

        private void SetupViewport()
        {
            GL.ClearColor(Color.Beige);

            Camera = Matrix4.CreateTranslation(0, -100, -1400) * Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1, 10000);

            GenericRenderer = new GenericRenderer();
        }

        private void Viewport_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref Camera);

            GenericRenderer.RenderLegacy();

            Viewport.SwapBuffers();
        }

        private void Viewport_Load(object sender, EventArgs e)
        {
            SetupViewport();
        }

        private void Viewport_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }
    }
}
