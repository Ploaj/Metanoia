using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Modeling
{
    public struct GenericVertex
    {
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
    }

    public class GenericMesh
    {
        public string Name;

        public List<GenericVertex> Vertices = new List<GenericVertex>();
        public List<uint> Triangles = new List<uint>();

        public GenericMaterial Material;
    }

    public class GenericMaterial
    {
        public GenericTexture TextureDiffuse;

        public TextureWrapMode SWrap { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode TWrap { get; set; } = TextureWrapMode.Repeat;
    }
}
