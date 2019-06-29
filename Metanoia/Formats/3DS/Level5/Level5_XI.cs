using Metanoia.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Metanoia.Formats._3DS.Level5
{
    public class Level5_XI
    {
        public int Width { get; set; }
        public int Height { get; set; }

        List<int> Tiles { get; set; } = new List<int>();

        public byte ImageFormat { get; set; }

        public byte[] ImageData { get; set; }

        public static Bitmap ToBitmap(byte[] xifile)
        {
            Level5_XI xi = new Level5_XI();
            xi.Open(xifile);
            return xi.ToBitmap();
        }

        public void Open(byte[] data)
        {
            using (DataReader r = new DataReader(new MemoryStream(data)))
            {
                r.Seek(0x10);
                Height = r.ReadInt16();
                Width = r.ReadInt16();

                r.Seek(0x1C);
                int someTable = r.ReadInt16();

                r.Seek(0x38);
                int someTableSize = r.ReadInt32();

                int imageDataOffset = someTable + someTableSize;

                byte[] _tileData = Decompress.Level5Decom(r.GetSection((uint)someTable, someTableSize));

                using (DataReader tileData = new DataReader(new MemoryStream(_tileData)))
                {
                    int tileCount = 0;
                    while (tileData.Position + 2 <= tileData.BaseStream.Length)
                    {
                        int i = tileData.ReadInt16();
                        if (i > tileCount) tileCount = i;
                        Tiles.Add(i);
                    }
                }

                r.Seek(0xA);
                int type = r.ReadByte(); switch (type)
                {
                    case 0x1:
                        type = 0x4;
                        break;
                    case 0x3:
                        type = 0x1;
                        break;
                    case 0x4:
                        type = 0x3;
                        break;
                    case 0x1B:
                        type = 0xC;
                        break;
                    case 0x1C:
                        type = 0xD;
                        break;
                    default:
                        throw new Exception("Unknown Texture Type " + type.ToString("x"));
                        //break;
                }
                ImageFormat = (byte)type;

                ImageData = Decompress.Level5Decom(r.GetSection((uint)imageDataOffset, (int)(r.BaseStream.Length - imageDataOffset)));
                
            }
        }

        public Bitmap ToBitmap()
        {
            int he = (int)Math.Ceiling(((Tiles.Count) * 8f) / 128);
            
            Bitmap tileSheet = _3DSImageTools.DecodeImage(ImageData, Tiles.Count * 8, 8, (_3DSImageTools.Tex_Formats)ImageFormat);
            var img = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            int[] srcpix = new int[tileSheet.Width * tileSheet.Height];
            int[] pixels = new int[Width * Height];

            BitmapData bmpData = tileSheet.LockBits(new Rectangle(0, 0, tileSheet.Width, tileSheet.Height), ImageLockMode.WriteOnly, tileSheet.PixelFormat);
            Marshal.Copy(bmpData.Scan0, srcpix, 0, srcpix.Length);
            tileSheet.UnlockBits(bmpData);

            int y = 0;
            int x = 0;

            for (int i = 0; i < Tiles.Count; i++)
            {
                int code = Tiles[i];

                int x1 = (code % 128) * 8;
                int y1 = (code / 128) * 8;

                if (code != -1)
                {
                    for (int h = 0; h < 8; h++)
                        for (int w = 0; w < 8; w++)
                            pixels[(x + w) + (y + h) * Width] = srcpix[(code * 8 + w) + (h) * tileSheet.Width];
                }
                if (code == -1 && (ImageFormat == 0xC || ImageFormat == 0xD))
                {
                    for (int h = 0; h < 8; h++)
                        for (int w = 0; w < 8; w++)
                            pixels[(x + w) + (y + h) * Width] = 0;
                }
                y += 8;

                if (y >= Height)
                {
                    y = 0;
                    x += 8;
                }
            }

            bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            img.UnlockBits(bmpData);

            img.RotateFlip(RotateFlipType.Rotate90FlipX);

            return img;
        }

    }
}
