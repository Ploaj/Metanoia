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
            this.exportedSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewerBox = new System.Windows.Forms.GroupBox();
            this.fileList = new System.Windows.Forms.ListView();
            this.folderTree = new System.Windows.Forms.TreeView();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(804, 24);
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
            // exportedSelectedToolStripMenuItem
            // 
            this.exportedSelectedToolStripMenuItem.Name = "exportedSelectedToolStripMenuItem";
            this.exportedSelectedToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.exportedSelectedToolStripMenuItem.Text = "Exported Selected";
            this.exportedSelectedToolStripMenuItem.Click += new System.EventHandler(this.exportedSelectedToolStripMenuItem_Click);
            // 
            // viewerBox
            // 
            this.viewerBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewerBox.Location = new System.Drawing.Point(470, 24);
            this.viewerBox.Name = "viewerBox";
            this.viewerBox.Size = new System.Drawing.Size(334, 417);
            this.viewerBox.TabIndex = 1;
            this.viewerBox.TabStop = false;
            this.viewerBox.Text = "Viewer";
            // 
            // fileList
            // 
            this.fileList.Dock = System.Windows.Forms.DockStyle.Left;
            this.fileList.Location = new System.Drawing.Point(301, 24);
            this.fileList.MultiSelect = false;
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(163, 417);
            this.fileList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.fileList.TabIndex = 2;
            this.fileList.UseCompatibleStateImageBehavior = false;
            this.fileList.View = System.Windows.Forms.View.List;
            this.fileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.fileList_MouseDoubleClick);
            // 
            // folderTree
            // 
            this.folderTree.Dock = System.Windows.Forms.DockStyle.Left;
            this.folderTree.Location = new System.Drawing.Point(0, 24);
            this.folderTree.Name = "folderTree";
            this.folderTree.Size = new System.Drawing.Size(295, 417);
            this.folderTree.TabIndex = 3;
            this.folderTree.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.folderTree_BeforeExpand);
            this.folderTree.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.folderTree_AfterExpand);
            this.folderTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.folderTree_AfterSelect);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(295, 24);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(6, 417);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(464, 24);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(6, 417);
            this.splitter2.TabIndex = 5;
            this.splitter2.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 441);
            this.Controls.Add(this.viewerBox);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.folderTree);
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
        private System.Windows.Forms.ListView fileList;
        private System.Windows.Forms.TreeView folderTree;
        private System.Windows.Forms.ToolStripMenuItem exportedSelectedToolStripMenuItem;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Splitter splitter2;
    }
}

