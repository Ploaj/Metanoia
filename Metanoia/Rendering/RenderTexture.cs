using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Metanoia.Rendering
{
    public class RenderTexture
    {
        public int GLID;

        public float Width, Height;

        public bool Loaded { get; internal set; } = false;

        public RenderTexture()
        {
            GL.GenTextures(1, out GLID);
        }

        public void Delete()
        {
            GL.DeleteTexture(GLID);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, GLID);
        }

        public void SetFromMaterial(GenericMaterial Material)
        {
            Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)Material.SWrap);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)Material.TWrap);
        }

        public void ExportPNG(string FileName)
        {
            int channels = 4;
            byte[] pixels = new byte[(int)(Width * Height * sizeof(byte) * channels)];
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            
            Bitmap b = new Bitmap((int)Width, (int)Height);
            BitmapData data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            
            b.UnlockBits(data);

            b.Save(FileName);
            b.Dispose();
        }

        public void LoadGenericTexture(GenericTexture Texture)
        {
            if (Texture.Mipmaps.Count == 0)
                return;

            GL.BindTexture(TextureTarget.Texture2D, GLID);

            //todo: compressed
            PixelType PixelType = PixelType.UnsignedByte;

            if (Texture.InternalFormat == PixelInternalFormat.Rgb5A1)
                PixelType = PixelType.UnsignedShort5551;

            if(Texture.InternalFormat == PixelInternalFormat.CompressedRgbS3tcDxt1Ext
                || Texture.InternalFormat == PixelInternalFormat.CompressedRgbaS3tcDxt5Ext)
            {
                GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, (InternalFormat)Texture.InternalFormat, (int)Texture.Width, (int)Texture.Height, 0, Texture.Mipmaps[0].Length, Texture.Mipmaps[0]);
                Console.WriteLine(GL.GetError() + " " + Texture.Mipmaps[0].Length.ToString("X"));
            }
            else
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, Texture.InternalFormat, (int)Texture.Width, (int)Texture.Height, 0, Texture.PixelFormat, PixelType, Texture.Mipmaps[0]);
            }

            Width = Texture.Width;
            Height = Texture.Height;

            Console.WriteLine(GLID + " " + Texture.Width + " " + Texture.Height + " " + Texture.InternalFormat + " " + Texture.PixelFormat);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Loaded = true;
        }
    }
}
