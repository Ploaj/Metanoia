namespace Metanoia
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewerBox = new System.Windows.Forms.GroupBox();
            this.fileList = new System.Windows.Forms.ListBox();
            this.folderTree = new System.Windows.Forms.TreeView();
            this.exportedSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(744, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportedSelectedToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // viewerBox
            // 
            this.viewerBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.viewerBox.Location = new System.Drawing.Point(492, 27);
            this.viewerBox.Name = "viewerBox";
            this.viewerBox.Size = new System.Drawing.Size(240, 314);
            this.viewerBox.TabIndex = 1;
            this.viewerBox.TabStop = false;
            this.viewerBox.Text = "Viewer";
            // 
            // fileList
            // 
            this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.fileList.FormattingEnabled = true;
            this.fileList.Location = new System.Drawing.Point(307, 27);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(179, 316);
            this.fileList.TabIndex = 2;
            this.fileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.fileList_MouseDoubleClick);
            // 
            // folderTree
            // 
            this.folderTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.folderTree.Location = new System.Drawing.Point(0, 27);
            this.folderTree.Name = "folderTree";
            this.folderTree.Size = new System.Drawing.Size(301, 314);
            this.folderTree.TabIndex = 3;
            this.folderTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.folderTree_BeforeExpand);
            this.folderTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.folderTree_AfterExpand);
            this.folderTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.folderTree_AfterSelect);
            // 
            // exportedSelectedToolStripMenuItem
            // 
            this.exportedSelectedToolStripMenuItem.Name = "exportedSelectedToolStripMenuItem";
            this.exportedSelectedToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.exportedSelectedToolStripMenuItem.Text = "Exported Selected";
            this.exportedSelectedToolStripMenuItem.Click += new System.EventHandler(this.exportedSelectedToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 353);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.folderTree);
            this.Controls.Add(this.viewerBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Metanoia";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.GroupBox viewerBox;
        private System.Windows.Forms.ListBox fileList;
        private System.Windows.Forms.TreeView folderTree;
        private System.Windows.Forms.ToolStripMenuItem exportedSelectedToolStripMenuItem;
    }
}

