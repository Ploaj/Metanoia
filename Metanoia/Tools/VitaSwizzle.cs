using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metanoia.Tools
{
    public class VitaSwizzle
    {
        public static int calcZOrder(int xPos, int yPos)
        {
            int[] MASKS = { 0x55555555, 0x33333333, 0x0F0F0F0F, 0x00FF00FF };
            int[] SHIFTS = { 1, 2, 4, 8 };

            int x = xPos;
            int y = yPos;

            x = (x | (x << SHIFTS[3])) & MASKS[3];
            x = (x | (x << SHIFTS[2])) & MASKS[2];
            x = (x | (x << SHIFTS[1])) & MASKS[1];
            x = (x | (x << SHIFTS[0])) & MASKS[0];

            y = (y | (y << SHIFTS[3])) & MASKS[3];
            y = (y | (y << SHIFTS[2])) & MASKS[2];
            y = (y | (y << SHIFTS[1])) & MASKS[1];
            y = (y | (y << SHIFTS[0])) & MASKS[0];

            int result = x | (y << 1);
            return result;
        }

        public static byte[] Deswizzle(byte[] data, int width, int height)
        {
            byte[] o = new byte[data.Length];

            for(int i = 0; i < data.Length; i += 8)
            {
                int z = calcZOrder(i / 8, 0);
                Array.Copy(data, i, o, z, 8);
            }

            return o;
        }

        public static byte[] UnswizzlePS4(byte[] buffer, int width, int height)
        {
            width >>= 2;
            height >>= 2;
            var PS4Tile = new byte[] {
            0, 1, 8, 9, 2, 3, 10, 11,
           16, 17, 24, 25, 18, 19, 26, 27,
            4, 5, 12, 13, 6, 7, 14, 15,
           20, 21, 28, 29, 22, 23, 30, 31,
           32, 33, 40, 41, 34, 35, 42, 43,
           48, 49, 56, 57, 50, 51, 58, 59,
           36, 37, 44, 45, 38, 39, 46, 47,
           52, 53, 60, 61, 54, 55, 62, 63
            };
            var bpb = 8;
            var tileWidth = 8;
            var tileHeight = 8;
            var tileSize = tileWidth * tileHeight;
            int width_real;
            int height_real;
            int width_show;
            int height_show;
            // untiling setup
            if ((width % tileWidth) != 0 || (height % tileHeight) != 0)
            {
                width_show = width;
                height_show = height;
                width = width_real = ((width + (tileWidth - 1)) / tileWidth) * tileWidth;
                height = height_real = ((height + (tileHeight - 1)) / tileHeight) * tileHeight;
            }
            else
            {
                width_show = width_real = width;
                height_show = height_real = height;
            }
            var RowSize = bpb * width;
            var o = new byte[buffer.Length];
            // untiling
            for (var InY = 0; InY < height; InY++)
                for (var InX = 0; InX < width; InX++)
                {
                    var Z = InY * width + InX;
                    var globalX = (Z / tileSize) * tileWidth;
                    var globalY = (globalX / width) * tileHeight;
                    globalX %= width;
                    var inTileX = Z % tileWidth;
                    var inTileY = (Z / tileWidth) % tileHeight;
                    var inTilePixel = inTileY * tileHeight + inTileX;
                    inTilePixel = PS4Tile[inTilePixel];
                    inTileX = inTilePixel % tileWidth;
                    inTileY = inTilePixel / tileHeight;
                    var OutX = globalX + inTileX;
                    var OutY = globalY + inTileY;
                    var PixelOffset_In = InX * bpb + InY * RowSize;
                    var PixelOffset_Out = OutX * bpb + OutY * RowSize;
                    for (int p = 0; p < bpb; p++)
                    {
                        o[PixelOffset_Out + p] = buffer[PixelOffset_In + p];
                    }
                }
            if(width_show != width_real || height_show != height_real)
            {
                Console.WriteLine("NOT CROPPED");
            }
            /*# crop
            
            if width_show != width_real or height_show != height_real:
                crop = bytearray(width_show * height_show * bpb)
                ReadInOneRow = width_show * bpb
                for Y in range(0, height_show):
                    Offset_IN = Y * RowSize
                    Offset_OUT = Y * width_show * bpb
                    crop[Offset_OUT: Offset_OUT + ReadInOneRow] = out[Offset_IN: Offset_IN + ReadInOneRow]
                o = crop;*/
            return o;
        }
    }
}
