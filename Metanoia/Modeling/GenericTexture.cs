using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.ComponentModel;

namespace Metanoia.Modeling
{
    public class GenericTexture
    {
        public string Name { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public int Id;

        public List<byte[]> Mipmaps = new List<byte[]>();

        [Browsable(false)]
        public PixelInternalFormat InternalFormat { get; set; } = PixelInternalFormat.Rgba;

        [Browsable(false)]
        public PixelFormat PixelFormat { get; set; } = PixelFormat.Bgra;

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
