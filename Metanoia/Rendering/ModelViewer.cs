using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;
using Metanoia.GUI;

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
                return Matrix4.CreateTranslation(_translation);
            }
            set
            {
                _translation = value.ExtractTranslation();
                UpdateCamera();
            }
        }

        private Matrix4 Transform
        {
            get
            {
                return Matrix4.CreateRotationZ(_rotation.Z) * Matrix4.CreateRotationY(_rotation.Y) * Matrix4.CreateRotationX(_rotation.X);
            }
        }

        public float X
        {
            get
            {
                return _translation.X;
            }
            set
            {
                _translation.X = value;
                UpdateCamera();
            }
        }
        public float Y
        {
            get
            {
                return _translation.Y;
            }
            set
            {
                _translation.Y = value;
                UpdateCamera();
            }
        }
        public float Z
        {
            get
            {
                return _translation.Z;
            }
            set
            {
                _translation.Z = value;
                UpdateCamera();
            }
        }
        private Vector3 _translation = Vector3.Zero;

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

        private Vector3 _defaultTranslation = new Vector3(0, -50, -100);
        private Vector3 _defaultRotation = new Vector3(0, 0, 0);

        private bool ShowBones = true;

        public ModelViewer()
        {
            InitializeComponent();

            foreach (var value in Enum.GetValues(typeof(RenderMode)))
            {
                renderMode.ComboBox.Items.Add(value);
            }
            renderMode.ComboBox.SelectedIndex = 0;
        }

        private void UpdateCamera()
        {
            Camera = Transform * Translation * Perspective;
        }

        private void SetupViewport()
        {
            GL.ClearColor(Color.DarkSlateGray);

            Translation = Matrix4.CreateTranslation(_defaultTranslation);

            GenericRenderer = new GenericRenderer();

            renderMode.ComboBox.SelectedValueChanged += UpdateRenderMode;//.DataBindings.Add("SelectedValue", GenericRenderer, "RenderMode");
        }

        private void UpdateRenderMode(object sender, EventArgs args)
        {
            GenericRenderer.RenderMode = (RenderMode)renderMode.SelectedItem;
            Viewport.Invalidate();
        }

        public void SetModel(GenericModel model)
        {
            if (model == null )
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

            if(model.Meshes.Count != 0)
            {
                _translation = new Vector3(0, -center.Y, -maxY * 1.5f);
                _rotation = Vector3.Zero;
                _defaultTranslation = _translation;
                _defaultRotation = _rotation;
                UpdateCamera();
            }

            GenericRenderer.SetGenericModel(model);
            ModelPanel.SetModel(model);
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
            
            GenericRenderer.RenderShader(Camera, ShowBones);

            Viewport.SwapBuffers();
        }

        private void RenderFloor()
        {
            GL.PushAttrib(AttribMask.AllAttribBits);

            int size = 50;
            int space = 5;

            GL.LineWidth(1f);
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
            UpdateCamera();
            Viewport.Invalidate();
        }

        private int PrevX, PrevY;

        private ModelInfoPanel ModelPanel = new ModelInfoPanel();

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            _translation = _defaultTranslation;
            _rotation = _defaultRotation;
            UpdateCamera();
            Viewport.Invalidate();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (!ModelPanel.Visible)
            {
                ModelPanel.Show();
            }
        }

        private void showBoneButton_Click(object sender, EventArgs e)
        {
            if (ShowBones)
            {
                showBoneButton.Image = Properties.Resources.icon_bone_off;
                ShowBones = false;
            }
            else
            {
                showBoneButton.Image = Properties.Resources.icon_bone_on;
                ShowBones = true;
            }
            RefreshRender();
        }
        
        private void Viewport_KeyDown(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            float speed = Vector3.TransformPosition(Vector3.Zero, Camera).LengthFast / 10;
            if(e.KeyChar == 'w')
                Z += speed;
            if (e.KeyChar == 's')
                Z -= speed;
            RefreshRender();
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                YRotation -= (PrevX - e.X) / 50f;
                XRotation -= (PrevY - e.Y) / 50f;
            }
            if (e.Button == MouseButtons.Right)
            {
                X -= (PrevX - e.X) / 2;
                Y += (PrevY - e.Y) / 2;
            }
            PrevX = e.X;
            PrevY = e.Y;
            Viewport.Invalidate();
        }
    }
}
