using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Modeling
{
    public struct GenericVertex
    {
        public const int Stride = (3 + 3 + 2 + 4 + 4 + 4) * 4;
        public Vector3 Pos;
        public Vector3 Nrm;
        public Vector2 UV0;
        public Vector4 Clr;
        public Vector4 Bones;
        public Vector4 Weights;
    }

    public class GenericModel
    {
        public string Name;

        public List<GenericMesh> Meshes = new List<GenericMesh>();

        public GenericSkeleton Skeleton;

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
    }

    public class GenericMesh
    {
        public string Name;

        public List<GenericVertex> Vertices = new List<GenericVertex>();
        public List<uint> Triangles = new List<uint>();

        public List<GenericMorph> Morphs = new List<GenericMorph>();

        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

        public GenericMaterial Material;

        public void Optimize()
        {
            MakeTriangles();

            Dictionary<GenericVertex, uint> vertices = new Dictionary<GenericVertex, uint>();

            Triangles.Clear();

            foreach(var v in Vertices)
            {
                if (!vertices.ContainsKey(v))
                    vertices.Add(v, (uint)vertices.Count);

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
        public GenericTexture TextureDiffuse;

        public TextureWrapMode SWrap { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode TWrap { get; set; } = TextureWrapMode.Repeat;
    }
}
