using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.ComponentModel;
using Metanoia.Tools;

namespace Metanoia.Modeling
{
    public struct GenericVertex
    {
        public const int Stride = (3 + 3 + 2 + 2 + 2 + 4 + 4 + 4) * 4;
        public Vector3 Pos;
        public Vector3 Nrm;
        public Vector2 UV0;
        public Vector2 UV1;
        public Vector2 UV2;
        public Vector4 Clr;
        public Vector4 Bones;
        public Vector4 Weights;
    }

    public class GenericModel
    {
        public string Name;

        public GenericSkeleton Skeleton { get; set; }

        public List<GenericMesh> Meshes = new List<GenericMesh>();

        public Dictionary<string, GenericTexture> TextureBank = new Dictionary<string, GenericTexture>();

        public Dictionary<string, GenericMaterial> MaterialBank = new Dictionary<string, GenericMaterial>();

        public bool HasMorphs
        {
            get
            {
                foreach (var m in Meshes)
                    if (m.Morphs.Count > 0)
                        return true;
                return false;
            }
        }

        public GenericMaterial GetMaterial(GenericMesh mesh)
        {
            if (mesh.MaterialName != null && MaterialBank.ContainsKey(mesh.MaterialName))
                return MaterialBank[mesh.MaterialName];

            return null;
        }

        public GenericTexture GetDiffuseTexture(GenericMesh mesh)
        {
            var material = GetMaterial(mesh);
            if (material != null && material.TextureDiffuse != null && TextureBank.ContainsKey(material.TextureDiffuse))
                return TextureBank[material.TextureDiffuse];

            return null;
        }

        public GenericTexture GetTexture(GenericMaterial material)
        {
            if (material.TextureDiffuse != null && TextureBank.ContainsKey(material.TextureDiffuse))
                return TextureBank[material.TextureDiffuse];

            return null;
        }
    }

    public class GenericMesh
    {
        [ReadOnly(true), Category("Properties")]
        public string Name { get; set; }

        [ReadOnly(true), Category("Data")]
        public List<GenericVertex> Vertices { get; set; } = new List<GenericVertex>();

        [ReadOnly(true), Category("Data")]
        public List<uint> Triangles = new List<uint>();

        [ReadOnly(true), Category("Data")]
        public List<GenericMorph> Morphs { get; set; } = new List<GenericMorph>();

        [ReadOnly(true), Category("Properties")]
        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

        [ReadOnly(true), Category("Properties")]
        public string MaterialName { get; set; }
        
        [ReadOnly(true), Category("Properties")]
        public int VertexCount { get { return Vertices.Count; } }

        [ReadOnly(true), Category("Properties")]
        public int TriangleCount { get { return Triangles.Count; } }

        [Category("Rendering")]
        public bool Visible { get; set; } = true;

        private bool CalculatedBounding = false;
        private Vector4 Bounding = Vector4.Zero;

        public Vector4 GetBounding()
        {
            if (!CalculatedBounding)
            {
                var positions = new List<Vector3>();
                foreach(var v in Vertices)
                {
                    positions.Add(v.Pos);
                }
                Bounding = BoundingSphereGenerator.GenerateBoundingSphere(positions);

                CalculatedBounding = true;
            }
            return Bounding;
        }

        public void Optimize()
        {
            MakeTriangles();

            Dictionary<GenericVertex, uint> vertices = new Dictionary<GenericVertex, uint>();

            Triangles.Clear();

            foreach(var v in Vertices)
            {
                if (!vertices.ContainsKey(v))
                    vertices.Add(v, (uint)vertices.Count);

                if (vertices.ContainsKey(v))
                    Triangles.Add(vertices[v]);
            }

            Vertices.Clear();
            Vertices.AddRange(vertices.Keys);
        }

        public void MakeTriangles()
        {
            var newTri = new List<uint>();
            if(PrimitiveType == PrimitiveType.TriangleStrip)
            {
                for(int index = 0; index < Triangles.Count-2; index ++)
                {
                    if (index % 2 != 1)
                    {
                        newTri.Add(Triangles[index]);
                        newTri.Add(Triangles[index+1]);
                        newTri.Add(Triangles[index+2]);
                    }
                    else
                    {
                        newTri.Add(Triangles[index + 2]);
                        newTri.Add(Triangles[index + 1]);
                        newTri.Add(Triangles[index]);
                    }
                }
            }
            if (PrimitiveType == PrimitiveType.Triangles)
                newTri = Triangles;
            Triangles = newTri;
        }
    }

    public class GenericMaterial
    {
        public string TextureDiffuse { get; set; }

        public TextureWrapMode SWrap { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode TWrap { get; set; } = TextureWrapMode.Repeat;

        public bool EnableBlend = true;
    }
}
