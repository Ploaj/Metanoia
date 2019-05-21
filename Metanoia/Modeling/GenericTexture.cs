using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

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

        public void FromBitmap(Bitmap image)
        {
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            InternalFormat = PixelInternalFormat.Rgba;
            PixelFormat = PixelFormat.Bgra;

            System.Drawing.Imaging.BitmapData bData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int size = bData.Stride * bData.Height;
            
            byte[] data = new byte[size];
            
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            Mipmaps.Add(data);

            image.UnlockBits(bData);
        }
    }
}
