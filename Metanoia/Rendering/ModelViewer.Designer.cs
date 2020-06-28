using OpenTK.Graphics;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelViewer));
            this.Viewport = new OpenTK.GLControl(new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 16));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.importAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.renderMode = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showBoneButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.animationTS = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.animationCB = new System.Windows.Forms.ToolStripComboBox();
            this.buttonBegin = new System.Windows.Forms.ToolStripButton();
            this.buttonPrevious = new System.Windows.Forms.ToolStripButton();
            this.buttonPlay = new System.Windows.Forms.ToolStripButton();
            this.buttonNext = new System.Windows.Forms.ToolStripButton();
            this.buttonEnd = new System.Windows.Forms.ToolStripButton();
            this.frameLabel = new System.Windows.Forms.ToolStripLabel();
            this.exportAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1.SuspendLayout();
            this.animationTS.SuspendLayout();
            this.SuspendLayout();
            // 
            // Viewport
            // 
            this.Viewport.BackColor = System.Drawing.Color.Black;
            this.Viewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Viewport.Location = new System.Drawing.Point(0, 50);
            this.Viewport.Name = "Viewport";
            this.Viewport.Size = new System.Drawing.Size(602, 312);
            this.Viewport.TabIndex = 0;
            this.Viewport.VSync = false;
            this.Viewport.Load += new System.EventHandler(this.Viewport_Load);
            this.Viewport.Paint += new System.Windows.Forms.PaintEventHandler(this.Viewport_Paint);
            this.Viewport.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Viewport_KeyDown);
            this.Viewport.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Viewport_MouseMove);
            this.Viewport.Resize += new System.EventHandler(this.Viewport_Resize);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.exportButton,
            this.toolStripSeparator4,
            this.toolStripButton1,
            this.toolStripButton3,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.renderMode,
            this.toolStripSeparator2,
            this.showBoneButton,
            this.toolStripSeparator3,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(602, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportModelToolStripMenuItem,
            this.importAnimationToolStripMenuItem,
            this.exportAnimationToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(38, 22);
            this.toolStripDropDownButton1.Text = "File";
            // 
            // importAnimationToolStripMenuItem
            // 
            this.importAnimationToolStripMenuItem.Name = "importAnimationToolStripMenuItem";
            this.importAnimationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.importAnimationToolStripMenuItem.Text = "Import Animation(s)";
            this.importAnimationToolStripMenuItem.Click += new System.EventHandler(this.importAnimationToolStripMenuItem_Click);
            // 
            // exportButton
            // 
            this.exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.exportButton.Image = global::Metanoia.Properties.Resources.icon_export;
            this.exportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(23, 22);
            this.exportButton.Text = "export button";
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Metanoia.Properties.Resources.icon_view;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            this.toolStripButton1.ToolTipText = "Reset View";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::Metanoia.Properties.Resources.icon_view;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(81, 22);
            this.toolStripLabel1.Text = "Render Mode:";
            // 
            // renderMode
            // 
            this.renderMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.renderMode.Name = "renderMode";
            this.renderMode.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // showBoneButton
            // 
            this.showBoneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showBoneButton.Image = global::Metanoia.Properties.Resources.icon_bone_on;
            this.showBoneButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showBoneButton.Name = "showBoneButton";
            this.showBoneButton.Size = new System.Drawing.Size(23, 22);
            this.showBoneButton.Text = "toolStripButton3";
            this.showBoneButton.ToolTipText = "Show/Hide Bones";
            this.showBoneButton.Click += new System.EventHandler(this.showBoneButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::Metanoia.Properties.Resources.icon_about;
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.ToolTipText = "Model Information";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // animationTS
            // 
            this.animationTS.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.animationCB,
            this.buttonBegin,
            this.buttonPrevious,
            this.buttonPlay,
            this.buttonNext,
            this.buttonEnd,
            this.frameLabel});
            this.animationTS.Location = new System.Drawing.Point(0, 25);
            this.animationTS.Name = "animationTS";
            this.animationTS.Size = new System.Drawing.Size(602, 25);
            this.animationTS.TabIndex = 2;
            this.animationTS.Text = "toolStrip2";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(71, 22);
            this.toolStripLabel2.Text = "Animations:";
            // 
            // animationCB
            // 
            this.animationCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.animationCB.Name = "animationCB";
            this.animationCB.Size = new System.Drawing.Size(121, 25);
            this.animationCB.SelectedIndexChanged += new System.EventHandler(this.animationCB_SelectedIndexChanged);
            // 
            // buttonBegin
            // 
            this.buttonBegin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonBegin.Image = global::Metanoia.Properties.Resources.icon_begin;
            this.buttonBegin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonBegin.Name = "buttonBegin";
            this.buttonBegin.Size = new System.Drawing.Size(23, 22);
            this.buttonBegin.Text = "Start";
            this.buttonBegin.Click += new System.EventHandler(this.buttonBegin_Click);
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonPrevious.Image = global::Metanoia.Properties.Resources.icon_begin;
            this.buttonPrevious.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(23, 22);
            this.buttonPrevious.Text = "Previous";
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // buttonPlay
            // 
            this.buttonPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonPlay.Image = global::Metanoia.Properties.Resources.icon_play;
            this.buttonPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(23, 22);
            this.buttonPlay.Text = "Play/Pause";
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonNext.Image = global::Metanoia.Properties.Resources.icon_end;
            this.buttonNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(23, 22);
            this.buttonNext.Text = "Next";
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonEnd
            // 
            this.buttonEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonEnd.Image = global::Metanoia.Properties.Resources.icon_end;
            this.buttonEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonEnd.Name = "buttonEnd";
            this.buttonEnd.Size = new System.Drawing.Size(23, 22);
            this.buttonEnd.Text = "End";
            this.buttonEnd.Click += new System.EventHandler(this.buttonEnd_Click);
            // 
            // frameLabel
            // 
            this.frameLabel.Name = "frameLabel";
            this.frameLabel.Size = new System.Drawing.Size(69, 22);
            this.frameLabel.Text = "Frame: 0 / 0";
            // 
            // exportAnimationToolStripMenuItem
            // 
            this.exportAnimationToolStripMenuItem.Name = "exportAnimationToolStripMenuItem";
            this.exportAnimationToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.exportAnimationToolStripMenuItem.Text = "Export Animation";
            this.exportAnimationToolStripMenuItem.Click += new System.EventHandler(this.exportAnimationToolStripMenuItem_Click);
            // 
            // exportModelToolStripMenuItem
            // 
            this.exportModelToolStripMenuItem.Name = "exportModelToolStripMenuItem";
            this.exportModelToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.exportModelToolStripMenuItem.Text = "Export Model";
            this.exportModelToolStripMenuItem.Click += new System.EventHandler(this.exportModelToolStripMenuItem_Click);
            // 
            // ModelViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Viewport);
            this.Controls.Add(this.animationTS);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ModelViewer";
            this.Size = new System.Drawing.Size(602, 362);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.animationTS.ResumeLayout(false);
            this.animationTS.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenTK.GLControl Viewport;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox renderMode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton showBoneButton;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton exportButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStrip animationTS;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox animationCB;
        private System.Windows.Forms.ToolStripButton buttonBegin;
        private System.Windows.Forms.ToolStripButton buttonPrevious;
        private System.Windows.Forms.ToolStripButton buttonPlay;
        private System.Windows.Forms.ToolStripButton buttonNext;
        private System.Windows.Forms.ToolStripButton buttonEnd;
        private System.Windows.Forms.ToolStripLabel frameLabel;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem importAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAnimationToolStripMenuItem;
    }
}
