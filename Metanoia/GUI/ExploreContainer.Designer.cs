namespace Metanoia.GUI
{
    partial class ExploreContainer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExploreContainer));
            this.fileTree = new System.Windows.Forms.TreeView();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.buttonModel = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileTree
            // 
            this.fileTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTree.HideSelection = false;
            this.fileTree.ItemHeight = 24;
            this.fileTree.Location = new System.Drawing.Point(0, 25);
            this.fileTree.Name = "fileTree";
            this.fileTree.Size = new System.Drawing.Size(297, 292);
            this.fileTree.TabIndex = 0;
            this.fileTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTree_AfterSelect);
            this.fileTree.MouseUp += new System.Windows.Forms.MouseEventHandler(this.fileTree_MouseUp);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
            this.propertyGrid1.Location = new System.Drawing.Point(297, 25);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(203, 292);
            this.propertyGrid1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonModel});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(500, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // buttonModel
            // 
            this.buttonModel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonModel.Image = ((System.Drawing.Image)(resources.GetObject("buttonModel.Image")));
            this.buttonModel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonModel.Name = "buttonModel";
            this.buttonModel.Size = new System.Drawing.Size(93, 22);
            this.buttonModel.Text = "Open As Model";
            this.buttonModel.Click += new System.EventHandler(this.buttonModel_Click);
            // 
            // ExploreContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 317);
            this.Controls.Add(this.fileTree);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "ExploreContainer";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView fileTree;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton buttonModel;
    }
}