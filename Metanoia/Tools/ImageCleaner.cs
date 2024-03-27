using System.Drawing;

namespace Metanoia.Tools
{
    public class ImageCleaner
    {
        private Bitmap RGB565;
        private Bitmap ETC1;
        public Bitmap Result;
        private Color Hair;
        private Color Skin;
        private Color Eyes;

        public ImageCleaner(Bitmap _RGB565, Bitmap _ETC1)
        {
            RGB565 = _RGB565;
            ETC1 = _ETC1;
            Hair = RGB565.GetPixel(0, 0);
            Skin = RGB565.GetPixel(0, 1);
            Eyes = RGB565.GetPixel(2, 0);
        }

        public void Cleaner()
        {
            Result = new Bitmap(RGB565.Width, RGB565.Height);
            for (int y = 0; y < RGB565.Height; y++)
            {
                for (int x = 0; x < RGB565.Width; x++)
                {
                    if (x < 3 & y == 0 || x < 2 & y == 1 || x == 0 & y == 2)
                    {
                        Result.SetPixel(x, y, SwitchColor(3, 0));
                    }
                    else
                    {
                        Result.SetPixel(x, y, SwitchColor(x, y));
                    }
                }
            }
        }

        private Color SwitchColor(int x, int y)
        {
            if (RGB565.GetPixel(x, y).R > 0)
            {
                return Color.FromArgb(Hair.R * RGB565.GetPixel(x, y).R / 255, Hair.G * RGB565.GetPixel(x, y).R / 255, Hair.B * RGB565.GetPixel(x, y).R / 255);
            }
            else if (RGB565.GetPixel(x, y).G > 0)
            {
                return Color.FromArgb(Eyes.R * RGB565.GetPixel(x, y).G / 255, Eyes.G * RGB565.GetPixel(x, y).G / 255, Eyes.B * RGB565.GetPixel(x, y).G / 255);
            }
            else if (RGB565.GetPixel(x, y).B > 0)
            {
                return Color.FromArgb(Skin.R * RGB565.GetPixel(x, y).B / 255, Skin.G * RGB565.GetPixel(x, y).B / 255, Skin.B * RGB565.GetPixel(x, y).B / 255);
            }
            else
            {
                return ETC1.GetPixel(x, y);
            }

        }
    }
}
