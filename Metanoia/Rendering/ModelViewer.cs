using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;

namespace Metanoia.Rendering
{
    /// <summary>
    /// Simple model viewport for rendering generic models
    /// </summary>
    public partial class ModelViewer : UserControl
    {
        public Matrix4 Camera;

        private GenericRenderer GenericRenderer;
        
        public Matrix4 Translation { get
            {
                return _translation;
            }
            set
            {
                _translation = value;
                UpdateCamera();
            }
        }
        private Matrix4 _translation;

        private Matrix4 Transform
        {
            get
            {
                return Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationX(_rotation.X);
            }
        }

        public float XRotation
        {
            get
            {
                return _rotation.X;
            }
            set
            {
                _rotation.X = value;
                UpdateCamera();
            }
        }
        public float YRotation
        {
            get
            {
                return _rotation.Y;
            }
            set
            {
                _rotation.Y = value;
                UpdateCamera();
            }
        }
        public float ZRotation
        {
            get
            {
                return _rotation.Z;
            }
            set
            {
                _rotation.Z = value;
                UpdateCamera();
            }
        }
        private Vector3 _rotation = Vector3.Zero;

        private Matrix4 Perspective
        {
            get
            {
                return Matrix4.CreatePerspectiveFieldOfView(1.3f, Width / (float)Height, 1, 10000);
            }
        }

        public ModelViewer()
        {
            InitializeComponent();
        }

        private void UpdateCamera()
        {
            Camera = Transform * Translation * Perspective;
        }

        private void SetupViewport()
        {
            GL.ClearColor(Color.DarkSlateGray);

            Translation = Matrix4.CreateTranslation(0, -50, -100);

            GenericRenderer = new GenericRenderer();
        }

        public void SetModel(GenericModel model)
        {
            if (model == null)
                return;
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach(var mesh in model.Meshes)
            {
                foreach(var vertex in mesh.Vertices)
                {
                    min.X = Math.Min(min.X, vertex.Pos.X);
                    min.Y = Math.Min(min.Y, vertex.Pos.Y);
                    min.Z = Math.Min(min.Z, vertex.Pos.Z);
                    max.X = Math.Max(max.X, vertex.Pos.X);
                    max.Y = Math.Max(max.Y, vertex.Pos.Y);
                    max.Z = Math.Max(max.Z, vertex.Pos.Z);
                }
            }

            var center = (min + max) / 2;
            var maxX = Math.Max(Math.Abs(min.X), Math.Abs(max.X));
            var maxY = Math.Max(Math.Abs(min.Y), Math.Abs(max.Y));
            var maxZ = Math.Max(Math.Abs(min.Z), Math.Abs(max.Z));

            Translation = Matrix4.CreateTranslation(0, -center.Y, -maxY * 1.5f);
            _rotation = Vector3.Zero;

            GenericRenderer.SetGenericModel(model);
        }

        public void RefreshRender()
        {
            Viewport.Invalidate();
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

            RenderFloor();
            
            GenericRenderer.RenderShader(Camera);

            Viewport.SwapBuffers();
        }

        private void RenderFloor()
        {
            GL.PushAttrib(AttribMask.AllAttribBits);

            int size = 50;
            int space = 5;

            GL.Color3(Color.White);
            GL.Begin(PrimitiveType.Lines);

            for (int i = -size; i <= size; i+=space)
            {
                GL.Vertex3(-size, 0, i);
                GL.Vertex3(size, 0, i);

                GL.Vertex3(i, 0, -size);
                GL.Vertex3(i, 0, size);
            }

            GL.End();
            GL.PopAttrib();
        }

        private void Viewport_Load(object sender, EventArgs e)
        {
            SetupViewport();
        }

        private void Viewport_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            Camera = Translation * Transform * Perspective;
        }

        private int PrevX, PrevY;
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                YRotation -= (PrevX - e.X) / 50f;
                Viewport.Invalidate();

                XRotation -= (PrevY - e.Y) / 50f;
                Viewport.Invalidate();
            }
            PrevX = e.X;
            PrevY = e.Y;
        }
    }
}
