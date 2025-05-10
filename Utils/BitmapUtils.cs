using ComputeSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ShapeScape.Utils
{
    public static class BitmapUtils
    {
        public static Rgba32[,] Downscale(Rgba32[,] input, int factor)
        {
            if (factor <= 0)
                throw new ArgumentException("Factor must be > 0", nameof(factor));

            int inHeight = input.GetLength(0);
            int inWidth = input.GetLength(1);

            int outHeight = inHeight / factor;
            int outWidth = inWidth / factor;
            Rgba32[,] output = new Rgba32[outHeight, outWidth];

            for (int y = 0; y < outHeight; y++)
            {
                for (int x = 0; x < outWidth; x++)
                {
                    int sumR = 0, sumG = 0, sumB = 0, sumA = 0;

                    for (int dy = 0; dy < factor; dy++)
                    {
                        for (int dx = 0; dx < factor; dx++)
                        {
                            Rgba32 pixel = input[y * factor + dy, x * factor + dx];
                            sumR += pixel.R;
                            sumG += pixel.G;
                            sumB += pixel.B;
                            sumA += pixel.A;
                        }
                    }

                    int total = factor * factor;
                    byte avgR = (byte)(sumR / total);
                    byte avgG = (byte)(sumG / total);
                    byte avgB = (byte)(sumB / total);
                    byte avgA = (byte)(sumA / total);

                    output[y, x] = new Rgba32(avgR, avgG, avgB, avgA);
                }
            }

            return output;
        }

        /// <summary>
        /// Converts a given 2D array of Rgba32 representing a <see cref="ReadWriteTexture2D{T, TPixel}"/> to a bitmap
        /// </summary>
        public static Bitmap ToBitmap(this Rgba32[,] rgbaArray)
        {
            int width = rgbaArray.GetLength(1);   // X-axis
            int height = rgbaArray.GetLength(0);  // Y-axis
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            BitmapData bmpData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            int stride = bmpData.Stride;
            IntPtr ptr = bmpData.Scan0;
            int bytes = stride * height;
            byte[] pixelData = new byte[bytes];

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * stride;
                for (int x = 0; x < width; x++)
                {
                    Rgba32 rgba = rgbaArray[y, x]; // Swapped access: [y, x]

                    int i = rowOffset + x * 4;
                    pixelData[i + 0] = rgba.B;
                    pixelData[i + 1] = rgba.G;
                    pixelData[i + 2] = rgba.R;
                    pixelData[i + 3] = rgba.A;
                }
            }

            Marshal.Copy(pixelData, 0, ptr, bytes);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

    }
}
