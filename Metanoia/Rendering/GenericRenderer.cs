using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;
using System.Collections.Generic;

namespace Metanoia.Rendering
{
    public class GenericRenderer
    {

        private GenericSkeleton Skeleton = null;
        private GenericModel Model = null;

        private Dictionary<GenericTexture, RenderTexture> Textures = new Dictionary<GenericTexture, RenderTexture>();

        public void SetGenericModel(GenericModel Model)
        {
            ClearTextures();

            Skeleton = Model.Skeleton;
            this.Model = Model;

            // load Textures
            foreach(GenericMesh m in Model.Meshes)
            {
                if (m.Material != null && m.Material.TextureDiffuse != null && !Textures.ContainsKey(m.Material.TextureDiffuse))
                {
                    RenderTexture Texture = new RenderTexture();
                    Texture.LoadGenericTexture(m.Material.TextureDiffuse);
                    Textures.Add(m.Material.TextureDiffuse, Texture);
                }
            }

            System.Console.WriteLine($"Loaded {Textures.Count} Textures");
        }

        public void ClearTextures()
        {
            foreach(RenderTexture texture in Textures.Values)
            {
                texture.Delete();
            }
            Textures.Clear();
        }

        public void RenderLegacy()
        {
            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.DepthTest);
            GL.Color3(1f, 1f, 1f);

            if (Model != null)
                foreach (GenericMesh mesh in Model.Meshes)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    if (Textures.ContainsKey(mesh.Material.TextureDiffuse))
                        Textures[mesh.Material.TextureDiffuse].SetFromMaterial(mesh.Material);

                    GL.Begin(PrimitiveType.Triangles);
                    foreach (uint t in mesh.Triangles)
                    {
                        //GL.Color3(mesh.Vertices[(int)t].Nrm);
                        GL.TexCoord2(mesh.Vertices[(int)t].UV0);
                        GL.Vertex3(mesh.Vertices[(int)t].Pos);
                    }
                    GL.End();
                }
            
            /*if(Textures.Count > 0)
            {
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                Textures[Model.Meshes[0].Material.TextureDiffuse].Bind();

                GL.Color4(0, 1f, 0, 1f);
                GL.Begin(PrimitiveType.Quads);
                GL.TexCoord2(1, 1); GL.Vertex2(1, -1);
                GL.TexCoord2(0, 1); GL.Vertex2(-1, -1);
                GL.TexCoord2(0, 0); GL.Vertex2(-1, 1);
                GL.TexCoord2(1, 0); GL.Vertex2(1, 1);
                GL.End();
            }*/
            /*
            
            GL.PointSize(10f);
            GL.Disable(EnableCap.DepthTest);
             GL.Color3(1f, 0, 0);
            GL.Begin(PrimitiveType.Points);

            if(Skeleton != null)
            foreach (GenericBone bone in Skeleton.Bones)
            {
                GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, Skeleton.GetBoneTransform(bone)));
            }

            GL.End();

            GL.LineWidth(5f);
            GL.Color3(0, 1f, 0);
            GL.Begin(PrimitiveType.Lines);

            if (Skeleton != null)
                foreach (GenericBone bone in Skeleton.Bones)
                {
                    if (bone.ParentIndex == -1) continue;
                    GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, Skeleton.GetBoneTransform(bone)));
                    GL.Vertex3(Vector3.TransformPosition(Vector3.Zero, Skeleton.GetBoneTransform(Skeleton.Bones[bone.ParentIndex])));
                }

            GL.End();*/

        }
    }
}
