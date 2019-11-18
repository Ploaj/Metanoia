using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Tools
{
    public class ImageTools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap RGB8ToBitmap(byte[] data, int width, int height)
        {
            int[] pixels = new int[width * height];

            var img = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            for(int i = 0; i < pixels.Length; i++)
            {
                if (i * 3 + 3 > data.Length)
                    break;
                pixels[0] = ((255 & 0xFF) << 24) | ((data[i * 3] & 0xFF) << 24) | ((data[i * 3 + 1] & 0xFF) << 24) | ((data[i * 3 + 2] & 0xFF));
            }

            var bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, img.PixelFormat);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            img.UnlockBits(bmpData);

            return img;
        }

    }
}
