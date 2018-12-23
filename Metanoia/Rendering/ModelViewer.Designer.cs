namespace Metanoia.Rendering
{
    partial class ModelViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Viewport = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // Viewport
            // 
            this.Viewport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Viewport.BackColor = System.Drawing.Color.Black;
            this.Viewport.Location = new System.Drawing.Point(3, 3);
            this.Viewport.Name = "Viewport";
            this.Viewport.Size = new System.Drawing.Size(428, 356);
            this.Viewport.TabIndex = 0;
            this.Viewport.VSync = false;
            this.Viewport.Load += new System.EventHandler(this.Viewport_Load);
            this.Viewport.Paint += new System.Windows.Forms.PaintEventHandler(this.Viewport_Paint);
            this.Viewport.Resize += new System.EventHandler(this.Viewport_Resize);
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Viewport);
            this.Name = "ModelViewer";
            this.Size = new System.Drawing.Size(434, 362);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl Viewport;
    }
}
