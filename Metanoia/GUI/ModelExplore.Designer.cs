namespace Metanoia.GUI
{
    partial class ModelExplore
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.viewerBox = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // viewerBox
            // 
            this.viewerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewerBox.Location = new System.Drawing.Point(0, 0);
            this.viewerBox.Name = "viewerBox";
            this.viewerBox.Size = new System.Drawing.Size(572, 261);
            this.viewerBox.TabIndex = 0;
            this.viewerBox.TabStop = false;
            this.viewerBox.Text = "Render";
            // 
            // ModelExplore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 261);
            this.Controls.Add(this.viewerBox);
            this.Name = "ModelExplore";
            this.Text = "ModelExplore";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox viewerBox;
    }
}