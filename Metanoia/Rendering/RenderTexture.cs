using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Metanoia.Modeling;
using System.Drawing.Imaging;
using System.Drawing;

namespace Metanoia.Rendering
{
    public class RenderTexture
    {
        public int GLID;

        public float Width, Height;

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
            int fb;
            GL.GenFramebuffers(1, out fb);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fb);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, GLID, 0);

            Bitmap b = new Bitmap((int)Width, (int)Height);
            BitmapData data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.ReadPixels(0, 0, (int)Width, (int)Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            b.UnlockBits(data);

            b.Save(FileName);
            b.Dispose();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(fb);
        }

        public void LoadGenericTexture(GenericTexture Texture)
        {
            GL.BindTexture(TextureTarget.Texture2D, GLID);

            //todo: compressed
            PixelType PixelType = PixelType.UnsignedByte;

            if (Texture.InternalFormat == PixelInternalFormat.Rgb5A1)
                PixelType = PixelType.UnsignedShort5551;

            GL.TexImage2D(TextureTarget.Texture2D, 0, Texture.InternalFormat, (int)Texture.Width, (int)Texture.Height, 0, Texture.PixelFormat, PixelType, Texture.Mipmaps[0]);

            Width = Texture.Width;
            Height = Texture.Height;

            Console.WriteLine(GLID + " " + Texture.Width + " " + Texture.Height + " " + Texture.InternalFormat + " " + Texture.PixelFormat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }
    }
}
