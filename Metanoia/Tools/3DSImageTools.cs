using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Metanoia.Tools
{
    public class _3DSImageTools
    {
        public enum Tex_Formats
        {
            RGBA8 = 0x00,
            RGB8,
            RGBA5551,
            RGB565,
            RGBA4444,
            LA8,
            HILO8,
            L8,
            A8,
            LA4,
            L4,
            A4,
            ETC1,
            ETC1a4
        }

        public static int[] zorder = new int[]
        {
            0, 1, 4, 5, 16, 17, 20, 21,
            2, 3, 6, 7, 18, 19, 22, 23,
            8, 9, 12, 13, 24, 25, 28, 29,
            10, 11, 14, 15, 26, 27, 30, 31,
            32, 33, 36, 37, 48, 49, 52, 53,
            34, 35, 38, 39, 50, 51, 54, 55,
            40, 41, 44, 45, 56, 57, 60, 61,
            42, 43, 46, 47, 58, 59, 62, 63
        };

        public static Bitmap DecodeImage(byte[] data, int width, int height, Tex_Formats type)
        {
            if (type == Tex_Formats.ETC1) return decodeETC(data, width, height);
            if (type == Tex_Formats.ETC1a4) return decodeAlpha(data, width, height);

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            int[] pixels = new int[width * height];

            int p = 0;

            for (int h = 0; h < height; h += 8)
                for (int w = 0; w < width; w += 8)
                {
                    // 8x8 block
                    int[] colors = new int[64];
                    for (int i = 0; i < 64; i++)
                    {
                        switch (type)
                        {
                            case Tex_Formats.RGBA8: colors[i] = Decode8888(data[p++], data[p++], data[p++], data[p++]); break;
                            case Tex_Formats.RGB8: colors[i] = Decode888(data[p++], data[p++], data[p++]); break;
                            case Tex_Formats.RGBA5551: colors[i] = Decode5551(data[p++], data[p++]); break;
                            case Tex_Formats.RGB565: 
                                {
                                    try
                                    {
                                        colors[i] = _3DSImageTools.Decode565((int)data[p++], (int)data[p++]);
                                        break;
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        for (h = 0; h < height; h += 8)
                                        {
                                            for (w = 0; w < width; w += 8)
                                            {
                                                colors = new int[64];
                                                for (i = 0; i < 64; i++)
                                                {
                                                    if (type == _3DSImageTools.Tex_Formats.RGB565)
                                                    {
                                                        colors[i] = 100;
                                                    }
                                                }
                                            }
                                        }
                                        for (int bh = 0; bh < 8; bh++)
                                        {
                                            for (int bw = 0; bw < 8; bw++)
                                            {
                                            }
                                        }
                                    }
                                    BitmapData bmpData2 = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                                    Marshal.Copy(pixels, 0, bmpData2.Scan0, pixels.Length);
                                    bmp.UnlockBits(bmpData2);
                                    return bmp;
                                }
                            case Tex_Formats.RGBA4444: colors[i] = Decode4444(data[p++], data[p++]); break;
                            case Tex_Formats.LA8: colors[i] = DecodeLA8(data[p++], data[p++]); break;
                            case Tex_Formats.HILO8: colors[i] = decodeHILO8(data[p++], data[p++]); break;
                            case Tex_Formats.L8: colors[i] = DecodeL8(data[p++]); break;
                            case Tex_Formats.A8: colors[i] = DecodeA8(data[p++]); break;
                            case Tex_Formats.LA4: colors[i] = DecodeLA4(data[p++]); break;
                            case Tex_Formats.L4:
                                {
                                    colors[i++] = DecodeL8((data[p] & 0xF) | ((data[p] & 0xF) << 4));
                                    colors[i] = DecodeL8((data[p] & 0xF0) | ((data[p] & 0xF0) >> 4));
                                    p++;
                                    break;
                                }
                            case Tex_Formats.A4:
                                {
                                    colors[i++] = DecodeA8((data[p] & 0xF) | ((data[p] & 0xF) << 4));
                                    colors[i] = DecodeA8((data[p] & 0xF0) | ((data[p] & 0xF0) >> 4));
                                    p++;
                                    break;
                                }
                            default: throw new Exception("Unsuppored format " + type.ToString("x"));
                        }

                    }

                    for (int bh = 0; bh < 8; bh++)
                        for (int bw = 0; bw < 8; bw++)
                        {
                            pixels[((w + bw) + (h + bh) * width)] = colors[CalcZOrder(bw, bh)];
                        }
                }

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static byte[] EncodeImage(Bitmap img, Tex_Formats type)
        {
            //if (type == Tex_Formats.ETC1) return RG_ETC1.encodeETC(img);
            //if (type == Tex_Formats.ETC1a4) return RG_ETC1.encodeETCa4(img);

            List<byte> o = new List<byte>();

            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);
            int[] pixels = new int[img.Width * img.Height];
            Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);
            img.UnlockBits(bmpData);

            for (int h = 0; h < img.Height; h += 8)
                for (int w = 0; w < img.Width; w += 8)
                {
                    // 8x8 block
                    List<byte[]> colors = new List<byte[]>();
                    for (int bh = 0; bh < 8; bh++)
                        for (int bw = 0; bw < 8; bw++)
                        {
                            switch (type)
                            {
                                case Tex_Formats.RGBA8: colors.Add(encode8888(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.RGB8: colors.Add(encode8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.RGBA4444: colors.Add(encode4444(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.RGBA5551: colors.Add(encode5551(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.RGB565: colors.Add(encode565(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.LA8: colors.Add(encodeLA8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.HILO8: colors.Add(encodeHILO8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.L8: colors.Add(encodeL8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.A8: colors.Add(encodeA8(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.LA4: colors.Add(encodeLA4(pixels[(w + bw) + (h + bh) * img.Width])); break;
                                case Tex_Formats.L4:
                                    {
                                        colors.Add(new byte[] { (byte)((encodeL8(pixels[(w + bw) + (h + bh) * img.Width])[0] / 0x11) & 0xF | ((encodeL8(pixels[(w + bw) + (h + bh) * img.Width + 1])[0] / 0x11) << 4)) });
                                        bw++;
                                        break;
                                    }
                                case Tex_Formats.A4:
                                    {
                                        colors.Add(new byte[] { (byte)((encodeA8(pixels[(w + bw) + (h + bh) * img.Width])[0] / 0x11) & 0xF | ((encodeA8(pixels[(w + bw) + (h + bh) * img.Width + 1])[0] / 0x11) << 4)) });
                                        bw++;
                                        break;
                                    }
                            }

                        }

                    for (int bh = 0; bh < 8; bh++)
                        for (int bw = 0; bw < 8; bw++)
                        {
                            int pos = bw + bh * 8;
                            for (int i = 0; i < zorder.Length; i++)
                                if (zorder[i] == pos)
                                {
                                    if (type == Tex_Formats.L4 || type == Tex_Formats.A4) { i /= 2; bw++; }
                                    o.AddRange(colors[i]);
                                    break;
                                }
                        }
                }

            return o.ToArray();
        }

        public static int[] _shift = { 0x00, 0x01, 0x04, 0x05, 0x10, 0x11, 0x14, 0x15 };
        public static int CalcZOrder(int xPos, int yPos)
        {
            int x = _shift[xPos];
            int y = _shift[yPos] << 1;

            return x | y;
        }

        #region Decoding

        public static int Decode8888(int b1, int b2, int b3, int b4)
        {
            return (b1 << 24) | (b4 << 16) | (b3 << 8) | b2;
        }

        public static int Decode888(int b1, int b2, int b3)
        {
            return (255 << 24) | (b3 << 16) | (b2 << 8) | b1;
        }

        private static int Decode5551(int b1, int b2)
        {
            int bt = b1 | (b2 << 8);
            int fst = (bt & 0xF800) >> 8;
            int scn = (bt & 0x07C0) >> 3;
            int thd = (bt & 0x003E) << 2;
            int a = (bt & 0x0001) >> 0;

            return (a * 255 << 24) | (fst << 16) | (scn << 8) | (thd);
        }

        private static int Decode565(int b1, int b2)
        {
            int bt = (b2 << 8) | b1;

            int a = 255;
            int r = (bt >> 11) & 0x1F;
            int g = (bt >> 5) & 0x3F;
            int b = (bt) & 0x1F;

            r = (r << 3) | (r >> 2);
            g = (g << 2) | (g >> 4);
            b = (b << 3) | (b >> 2);

            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public static int Decode4444(int b1, int b2)
        {
            int a = (b1 & 0x0F) * 17;
            int r = (b2 & 0xF0);
            int g = (b2 & 0x0F) * 17;
            int b = (b1 & 0xF0);

            a = a | ((a) >> 4);
            r = r | ((r) >> 4);
            g = g | ((g) >> 4);
            b = b | ((b) >> 4);

            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        public static int DecodeL8(int b1)
        {
            return (255 << 24) | (b1 << 16) | (b1 << 8) | b1;
        }

        public static int DecodeA8(int b1)
        {
            return (b1 << 24) | (255 << 16) | (255 << 8) | 255;
        }

        public static int DecodeLA8(int b1, int b2)
        {
            return (b1 << 24) | (b2 << 16) | (b2 << 8) | b2;
        }

        public static int DecodeLA4(int b)
        {
            int r = b >> 4;
            int a = b & 0x0F;
            a = a | (a << 4);
            r = r | (r << 4);
            return (a << 24) | (r << 16) | (r << 8) | r;
        }

        public static int decodeHILO8(int b1, int b2)
        {
            return (255 << 24) | (b2 << 16) | (b1 << 8) | 255;
        }

        #endregion

        #region Encoding
        public static byte[] encode8888(int color)
        {
            return new byte[] { (byte)((color >> 24) & 0xFF), (byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF) };
        }

        public static byte[] encode8(int color)
        {
            return new byte[] { (byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF) };
        }

        public static byte[] encode4444(int color)
        {
            int val = 0;
            val += (((color >> 24) & 0xFF) / 0x11);
            val += ((((color) & 0xFF) / 0x11) << 4);
            val += ((((color >> 8) & 0xFF) / 0x11) << 8);
            val += ((((color >> 16) & 0xFF) / 0x11) << 12);
            return new byte[] { (byte)(val & 0xFF), (byte)(val >> 8) };
        }

        public static byte[] encodeA8(int color)
        {
            return new byte[] { (byte)((color >> 24) & 0xFF) };
        }

        public static byte[] encodeL8(int color)
        {
            return new byte[] { (byte)(((0x4CB2 * (color & 0xFF) + 0x9691 * ((color >> 8) & 0xFF) + 0x1D3E * ((color >> 8) & 0xFF)) >> 16) & 0xFF) };
        }

        public static byte calLum(int color)
        {
            return (byte)(((0x4CB2 * (color & 0xFF) + 0x9691 * ((color >> 8) & 0xFF) + 0x1D3E * ((color >> 8) & 0xFF)) >> 16) & 0xFF);
        }

        public static byte[] encodeLA4(int color)
        {
            return new byte[] { (byte)((((color >> 24) / 0x11) & 0xF | ((color >> 16) / 0x11) & 0xF << 4)) };
        }

        public static byte[] encodeLA8(int color)
        {
            return new byte[] { (byte)((color >> 24) & 0xFF), (byte)((color >> 16) & 0xFF) };
        }

        public static byte[] encodeHILO8(int color)
        {
            return new byte[] { (byte)((color) & 0xFF), (byte)((color >> 8) & 0xFF) };
        }

        public static byte[] encode565(int c)
        {
            int r = ((c >> 16) & 0xFF) >> 3;
            int g = ((c >> 8) & 0xFF) >> 2;
            int b = ((c) & 0xFF) >> 3;
            int val = (r << 11) | (g << 5) | b;
            return new byte[] { (byte)(val & 0xFF), (byte)(val >> 8) };
        }

        public static byte[] encode5551(int c)
        {
            int val = 0;
            val += (byte)(((c >> 24) & 0xFF) > 0x80 ? 1 : 0);
            val += convert8to5(((c >> 16) & 0xFF)) << 11;
            val += convert8to5(((c >> 8) & 0xFF)) << 6;
            val += convert8to5(((c) & 0xFF)) << 1;
            ushort v = (ushort)val;
            return new byte[] { (byte)(val & 0xFF), (byte)(val >> 8) };
        }

        #endregion

        static byte convert8to5(int col)
        {
            byte[] Convert8to5 = { 0x00,0x08,0x10,0x18,0x20,0x29,0x31,0x39,
                                   0x41,0x4A,0x52,0x5A,0x62,0x6A,0x73,0x7B,
                                   0x83,0x8B,0x94,0x9C,0xA4,0xAC,0xB4,0xBD,
                                   0xC5,0xCD,0xD5,0xDE,0xE6,0xEE,0xF6,0xFF };
            byte i = 0;
            while (col > Convert8to5[i]) i++;
            return i;
        }

        #region ETC

        public static Bitmap decodeETC(byte[] bytes, int width, int height)
        {
            int[] pixels = new int[width * height];

            int p = 0;
            int i, j;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    if (p + 8 > bytes.Length)
                        break;

                    byte[] temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);


                    int[] pix = decodeETCBlock(bytesToLong(temp), null);

                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 8 > bytes.Length)
                        break;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), null);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;


                }
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public static Bitmap decodeAlpha(byte[] bytes, int width, int height)
        {
            int[] pixels = new int[width * height];

            int p = 0;
            int i, j;

            for (i = 0; i < height; i += 8)
            {
                for (j = 0; j < width; j += 8)
                {
                    int x, y;

                    if (p + 16 > bytes.Length)
                        break;

                    byte[] alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    byte[] temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    int[] pix = decodeETCBlock(bytesToLong(temp), alpha);

                    int pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i; x < i + 4; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j; y < j + 4; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;

                    if (p + 16 > bytes.Length)
                        break;

                    alpha = new byte[8];
                    Buffer.BlockCopy(bytes, p, alpha, 0, 8);
                    p += 8;

                    temp = new byte[8];
                    Buffer.BlockCopy(bytes, p, temp, 0, 8);

                    pix = decodeETCBlock(bytesToLong(temp), alpha);

                    pi = 0;
                    for (x = i + 4; x < i + 8; x++)
                        for (y = j + 4; y < j + 8; y++)
                            pixels[y + x * width] = pix[pi++];
                    p += 8;


                }
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private static long bytesToLong(byte[] b)
        {
            long result = 0;
            for (int i = 7; i >= 0; i--)
            {
                result <<= 8;
                result |= (long)(b[i] & 0xFF);
            }
            return result;
        }

        public static Color[] DecodeETC1(ulong bl)
        {
            int[] b = decodeETCBlock((long)bl, null);

            Color[] c = new Color[b.Length];

            for (int i = 0; i < b.Length; i++)
                c[i] = Color.FromArgb((b[i] >> 24) & 0xFF, (b[i] >> 16) & 0xFF, (b[i] >> 8) & 0xFF, (b[i]) & 0xFF);

            return c;
        }

        private static int[] decodeETCBlock(long block, byte[] alpha)
        {

            /*
             * If flipbit=0, the block is divided into two 2x4 and 4x2 otherwise
             * Diffbit is for layouts
             */

            bool hasAlpha = alpha != null;

            int[,] aChannel = new int[4, 4];

            if (hasAlpha)
            {
                int ai = 0;
                for (int w = 0; w < 4; w++)
                    for (int h = 0; h < 4; h++)
                    {
                        aChannel[w, h++] = (alpha[ai] & 0x0F) * 17;
                        aChannel[w, h] = ((alpha[ai++] & 0xF0) >> 4) * 17;
                    }
            }

            int[,] codeWord = {{-8,-2,2,8},{-17,-5,5,17},{-29,-9,9,29},{-42,-13,13,42}
                            ,{-60,-18,18,60},{-80,-24,24,80},{-106,-33,33,106},{-183,-47,47,183}};

            bool flipbit = extendBits(block, 32, 1) == 1;
            bool diffbit = extendBits(block, 33, 1) == 1;

            //Note, extend 2 extends 3 but without double word
            int codeWord1 = extendBits(block, 37, 2);
            int codeWord2 = extendBits(block, 34, 2);

            /*
             *  In the 'individual' mode (diffbit = 0), the base color for
             *  subblock 1 is derived from the codewords R1 (bit 63-60), G1 (bit 55-52) and B1 (bit 47-44)
             */

            int r1, r2, g1, g2, b1, b2;

            if (diffbit == false)
            {

                r1 = extendBits(block, 60, 4);
                r2 = extendBits(block, 56, 4);
                g1 = extendBits(block, 52, 4);
                g2 = extendBits(block, 48, 4);
                b1 = extendBits(block, 44, 4);
                b2 = extendBits(block, 40, 4);

            }
            else
            {
                r1 = extendBits(block, 59, 5);
                g1 = extendBits(block, 51, 5);
                b1 = extendBits(block, 43, 5);

                r2 = extendBits(block, 59, 6) + extendBits(block, 56, 3);
                g2 = extendBits(block, 51, 6) + extendBits(block, 48, 3);
                b2 = extendBits(block, 43, 6) + extendBits(block, 40, 3);

                r2 = (r2 << 3) | (r2 >> 2);
                g2 = (g2 << 3) | (g2 >> 2);
                b2 = (b2 << 3) | (b2 >> 2);

            }

            //Now for making the pixels

            int[] difc = new int[16];

            int cw = 0;
            for (int w = 0; w < 4; w++)
            {
                for (int h = 0; h < 4; h++)
                {
                    switch ((extendBits(block, cw, 1)) | (extendBits(block, cw + 16, 1) << 1))
                    {
                        case 3:
                            difc[w + h * 4] = 0;
                            break;
                        case 2:
                            difc[w + h * 4] = 1;
                            break;
                        case 0:
                            difc[w + h * 4] = 2;
                            break;
                        case 1:
                            difc[w + h * 4] = 3;
                            break;
                    }

                    cw++;
                }
            }

            int[] pixels = new int[4 * 4];

            cw = 0;
            if (flipbit)
            {
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 4; w++)
                    {
                        if (h < 2)
                        {
                            int dif = codeWord[codeWord1, difc[h * 4 + w]];
                            int a = hasAlpha ? aChannel[w, h] : 255;
                            pixels[h * 4 + w] = (a << 24) | (clamp(r1 + dif) << 16) | (clamp(g1 + dif) << 8) | clamp(b1 + dif);
                        }
                        else
                        {
                            int dif = codeWord[codeWord2, difc[h * 4 + w]];
                            int a = hasAlpha ? aChannel[w, h] : 255;
                            pixels[h * 4 + w] = (a << 24) | (clamp(r2 + dif) << 16) | (clamp(g2 + dif) << 8) | clamp(b2 + dif);
                        }
                        cw++;
                    }
                }
            }
            else
            {
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 0; w < 2; w++)
                    {
                        int dif = codeWord[codeWord1, difc[h * 4 + w]];
                        int a = hasAlpha ? aChannel[w, h] : 255;
                        pixels[h * 4 + w] = (a << 24) | (clamp(r1 + dif) << 16) | (clamp(g1 + dif) << 8) | clamp(b1 + dif);
                        cw++;
                    }
                }
                for (int h = 0; h < 4; h++)
                {
                    for (int w = 2; w < 4; w++)
                    {
                        int dif = codeWord[codeWord2, difc[h * 4 + w]];
                        int a = hasAlpha ? aChannel[w, h] : 255;
                        pixels[h * 4 + w] = (a << 24) | (clamp(r2 + dif) << 16) | (clamp(g2 + dif) << 8) | clamp(b2 + dif);
                        cw++;
                    }
                }
            }

            return pixels;

        }

        private static int clamp(int n)
        {
            if (n > 255) n = 255;
            if (n < 0) n = 0;
            return n;
        }

        private static int extendBits(long n, int p, int amt)
        {
            int num;
            switch (amt)
            {
                case 1:
                    return (int)((n >> p) & 0x1);
                case 2:
                    num = (int)((n >> p) & 0x7);
                    return num;
                case 3:
                    int[] complement = { 0, 1, 2, 3, -4, -3, -2, -1 };
                    num = (int)((n >> p) & 0x7);
                    return complement[num];
                case 4:
                    num = (int)((n >> p) & 0xF);
                    return (num << 4) | (num);
                case 5:
                    num = (int)((n >> p) & 0x1F);
                    return (num << 3) | (num >> 2);
                case 6:
                    num = (int)((n >> p) & 0x1F);
                    return num & 0x1F;
                default:
                    return 0;
            }

        }
        #endregion
    }
}
