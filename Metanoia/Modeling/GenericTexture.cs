using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Metanoia.Rendering;

namespace Metanoia.Modeling
{
    public class GenericTexture
    {
        public string Name { get; set; }

        public uint Width { get; set; }

        public uint Height { get; set; }

        public byte PixelFormatTrue { get; set; }

        public int Id;

        public List<byte[]> Mipmaps = new List<byte[]>();

        [Browsable(false)]
        public PixelInternalFormat InternalFormat { get; set; } = PixelInternalFormat.Rgba;

        [Browsable(false)]
        public OpenTK.Graphics.OpenGL.PixelFormat PixelFormat { get; set; } = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

        public Bitmap GetBitmap(int mipLevel = 0)
        {
            if (Id == -1)
                return null;
            int channels = 4;
            byte[] data = new byte[Width * Height * sizeof(byte) * channels];
            
            GL.GetTexImage(TextureTarget.Texture2D, mipLevel, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data);

            return GetBitmap((int)Width, (int)Height, data);
        }

        public static Bitmap GetBitmap(int width, int height, byte[] imageData)
        {
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public void FromBitmap(Bitmap image)
        {
            Width = (uint)image.Width;
            Height = (uint)image.Height;

            InternalFormat = PixelInternalFormat.Rgba;
            PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

            BitmapData bData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
               ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int size = bData.Stride * bData.Height;
            
            byte[] data = new byte[size];
            
            Marshal.Copy(bData.Scan0, data, 0, size);

            Mipmaps.Add(data);

            image.UnlockBits(bData);
        }
    }
}
