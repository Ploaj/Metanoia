using Metanoia.Modeling;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metanoia.GUI
{
    public partial class ModelInfoPanel : Form
    {
        public ModelInfoPanel()
        {
            InitializeComponent();

            TopMost = true;


        }

        public void SetModel(GenericModel m)
        {
            treeView1.Nodes.Clear();
            
            if(m.Skeleton != null)
            {
                Dictionary<int, TreeNode> indexToBone = new Dictionary<int, TreeNode>();
                int boneIndex = 0;
                foreach (var bone in m.Skeleton.Bones)
                    indexToBone.Add(boneIndex++, new TreeNode() { Text = bone.Name, Tag = bone });
                boneIndex = 0;
                foreach (var bone in m.Skeleton.Bones)
                {
                    if (bone.ParentIndex == -1)
                        treeView1.Nodes.Add(indexToBone[boneIndex]);
                    else
                        indexToBone[bone.ParentIndex].Nodes.Add(indexToBone[boneIndex]);
                    boneIndex++;
                }
            }

            var textureNode = new TreeNode() { Text = "Textures" };
            foreach(var tex in m.TextureBank)
                textureNode.Nodes.Add(new TreeNode() { Text = tex.Key, Tag = tex.Value });
            treeView1.Nodes.Add(textureNode);

            var materialNode = new TreeNode() { Text = "Materials" };
            foreach (var tex in m.MaterialBank)
                materialNode.Nodes.Add(new TreeNode() { Text = tex.Key, Tag = tex.Value });
            treeView1.Nodes.Add(materialNode);

            var meshNode = new TreeNode() { Text = "Mesh" };
            foreach (var mesh in m.Meshes)
                meshNode.Nodes.Add(new TreeNode() { Text = mesh.Name, Tag = mesh });
            treeView1.Nodes.Add(meshNode);
        }

        private void ModelInfoPanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.Nodes.Count == 0)
                return;

            propertyGrid1.SelectedObject = e.Node.Tag;
            
            foreach(var node in GetAllNodes(treeView1.Nodes[0]))
            {
                if(node.Tag is GenericBone bone)
                {
                    bone.Selected = false;
                }
            }
            if (e.Node.Tag is GenericBone b)
            {
                b.Selected = true;
            }
        }

        private static List<TreeNode> GetAllNodes(TreeView _self)
        {
            List<TreeNode> result = new List<TreeNode>();
            foreach (TreeNode child in _self.Nodes)
            {
                result.AddRange(GetAllNodes(child));
            }
            return result;
        }

        private static List<TreeNode> GetAllNodes(TreeNode _self)
        {
            List<TreeNode> result = new List<TreeNode>();
            result.Add(_self);
            foreach (TreeNode child in _self.Nodes)
            {
                result.AddRange(GetAllNodes(child));
            }
            return result;
        }
    }
}
