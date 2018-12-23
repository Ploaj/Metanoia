using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Metanoia.Modeling
{
    public class GenericTexture
    {
        public string Name;

        public int Id;

        public List<byte[]> Mipmaps = new List<byte[]>();

        public uint Width { get; set; }
        public uint Height { get; set; }
        public PixelInternalFormat InternalFormat { get; set; }
        public PixelFormat PixelFormat { get; set; }
    }
}
