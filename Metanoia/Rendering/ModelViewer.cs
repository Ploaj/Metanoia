using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;
using Metanoia.GUI;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Metanoia.Formats;
using Metanoia.Formats._3DS.Level5;

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
                return Matrix4.CreatePerspectiveFieldOfView(1f, Width / (float)Height, 0.1f, 100000);
            }
        }

        private Vector3 _defaultTranslation = new Vector3(0, -50, -100);
        private Vector3 _defaultRotation = new Vector3(0, 0, 0);

        private bool ShowBones = true;


        private int Frame
        {
            get
            {
                return _frame;
            }
            set
            {
                _frame = value;
                frameLabel.Text = $"Frame: {_frame} / {MaxFrame}";
            }
        }
        private int _frame = 0;

        private int MaxFrame
        {
            get
            {
                return _maxFrame;
            }
            set
            {
                _maxFrame = value;
                frameLabel.Text = $"Frame: {Frame} / {_maxFrame}";
            }
        }
        private int _maxFrame = 0;

        private GenericModel Model { get; set; }

        public ModelViewer()
        {
            InitializeComponent();

            foreach (var value in Enum.GetValues(typeof(RenderMode)))
            {
                renderMode.ComboBox.Items.Add(value);
            }
            renderMode.ComboBox.SelectedIndex = 0;

            animationTS.Visible = false;

            System.Timers.Timer timer = new System.Timers.Timer();

            timer.Interval = 1000 / 60d;

            timer.Elapsed += (sender, args) =>
            {
                if (Viewport.IsDisposed)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                Viewport.Invalidate();
            };

            timer.Start();
        }

        private bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                _isPlaying = value;
                if (_isPlaying)
                    buttonPlay.Image = Properties.Resources.icon_pause;
                else
                    buttonPlay.Image = Properties.Resources.icon_play;
            }
        }
        private bool _isPlaying = false;
        

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

            var radius = Math.Max(maxY, Math.Max(maxX, maxZ));

            if(model.Meshes.Count != 0)
            {
                /*_translation = new Vector3(0, -center.Y, -maxY * 1.5f);
                _rotation = Vector3.Zero;
                _defaultTranslation = _translation;
                _defaultRotation = _rotation;

                _defaultTranslation = new Vector3(0, -center.Y, -radius * 1.5f);
                _defaultRotation = new Vector3(0.25f, 0.40f, 0);

                UpdateCamera();*/
            }

            Model = model;
            ModelPanel.SetModel(model);
        }

        /// <summary>
        /// 
        /// </summary>
        public void EnableAnimation()
        {
            animationTS.Visible = true;
            Frame = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddAnimation(GenericAnimation animation)
        {
            animationCB.Items.Add(animation);
            animationCB.SelectedItem = animation;
            EnableAnimation();
            if (Model.Skeleton != null)
                animation.UpdateSkeleton(Frame, Model.Skeleton);
            ResetView();
        }

        private void Viewport_Paint(object sender, PaintEventArgs e)
        {
            RenderScene();
        }

        private void RenderScene()
        {
            if (GenericRenderer == null)
            {
                return;
            }

            if (!GenericRenderer.HasModelSet)
            {
                GenericRenderer.SetGenericModel(Model);
            }

            if (IsPlaying)
                buttonNext_Click(null, null);

            Viewport.MakeCurrent();

            GL.Viewport(0, 0, Width, Height);

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

        private void Render(int width, int height)
        {
            GL.PushAttrib(AttribMask.AllAttribBits);
            // render stuff
            {
                GL.Viewport(0, 0, width, height);

                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);

                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                GenericRenderer.RenderShader(Camera, false);
            }
            GL.PopAttrib();

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            {
                // RGBA unsigned byte
                int pixelSizeInBytes = sizeof(byte) * 4;
                int imageSizeInBytes = width * height * pixelSizeInBytes;

                byte[] pixels = new byte[imageSizeInBytes];

                // Read the pixels from the framebuffer. PNG uses the BGRA format. 
                GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);

                bmp.UnlockBits(bmpData);

                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                bmp.Save("Render.png");

                bmp.Dispose();
            }
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
            UpdateCamera();
            Viewport.Invalidate();
        }

        private int PrevX, PrevY;

        private ModelInfoPanel ModelPanel = new ModelInfoPanel();

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ResetView();
            Viewport.Invalidate();
        }

        public void ResetView()
        {
            _translation = _defaultTranslation;
            _rotation = _defaultRotation;
            UpdateCamera();
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
        }
        
        private void Viewport_KeyDown(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            float speed = Vector3.TransformPosition(Vector3.Zero, Camera).LengthFast / 10;
            if(e.KeyChar == 'w')
                Z += speed;
            if (e.KeyChar == 's')
                Z -= speed;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Render(Viewport.Width, Viewport.Height);
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            ExportModel();
        }

        public void ExportModel()
        {
            using (SaveFileDialog d = new SaveFileDialog())
            {
                d.Filter = FormatManager.Instance.GetModelExportFilter();
                d.FileName = Model.Name;
                if (string.IsNullOrEmpty(d.FileName))
                    d.FileName = "model";

                if(d.ShowDialog() == DialogResult.OK)
                {
                    FormatManager.Instance.ExportModel(d.FileName, Model);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog d = new OpenFileDialog())
            {
                d.Multiselect = true;
                d.Filter = FormatManager.Instance.GetAnimationExtensionFilter();

                if (d.ShowDialog() == DialogResult.OK)
                {
                    foreach (String filename in d.FileNames)
                    {
                        if (Path.GetExtension(filename) == ".xc")
                        {
                            Level5_XC animArchive = new Level5_XC();
                            animArchive.Open(new FileItem(filename, File.ReadAllBytes(filename)));
                            GenericAnimation[] animations = animArchive.ToGenericAnimation();

                            if (animations != null)
                            {
                                foreach (GenericAnimation animation in animArchive.ToGenericAnimation())
                                {
                                    AddAnimation(animation);
                                }
                            }

                        } else if (FormatManager.Instance.Open(new FileItem(filename)) is IAnimationFormat anim)
                        {
                            AddAnimation(anim.ToGenericAnimation());
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void animationCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(animationCB.SelectedItem is GenericAnimation anim)
            {
                MaxFrame = anim.FrameCount;
                
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            Frame++;
            if (Frame >= MaxFrame)
                Frame = 0;
            if (Model.Skeleton != null && animationCB.SelectedItem is GenericAnimation anim)
                anim.UpdateSkeleton(Frame, Model.Skeleton);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            IsPlaying = !IsPlaying;
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            Frame--;
            if (Frame < 0)
                Frame = MaxFrame - 1;
            if (Model.Skeleton != null && animationCB.SelectedItem is GenericAnimation anim)
                anim.UpdateSkeleton(Frame, Model.Skeleton);
        }

        private void buttonBegin_Click(object sender, EventArgs e)
        {
            Frame = 0;
            if (Model.Skeleton != null && animationCB.SelectedItem is GenericAnimation anim)
                anim.UpdateSkeleton(Frame, Model.Skeleton);
        }

        private void buttonEnd_Click(object sender, EventArgs e)
        {
            Frame = MaxFrame;
            if (Model.Skeleton != null && animationCB.SelectedItem is GenericAnimation anim)
                anim.UpdateSkeleton(Frame, Model.Skeleton);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportModel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exportAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(animationCB.SelectedItem is GenericAnimation anim)
            {
                IsPlaying = false;
                Frame = 0;
                FormatManager.Instance.ExportAnimation(Model.Skeleton, anim);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            var camPos = Vector3.TransformPosition(Vector3.Zero, Camera);

            float speed = (float)Math.Sqrt(camPos.X * camPos.X + camPos.Z * camPos.Z) * 0.025f;

            if (e.Button == MouseButtons.Left)
            {
                YRotation -= (PrevX - e.X) / 50f;
                XRotation -= (PrevY - e.Y) / 50f;
            }
            if (e.Button == MouseButtons.Right)
            {
                X -= (PrevX - e.X) * 0.75f * speed;
                Y += (PrevY - e.Y) * 0.75f * speed;
            }
            PrevX = e.X;
            PrevY = e.Y;
        }
        // FrameViewport
        // MakeRender
        
    }
}
