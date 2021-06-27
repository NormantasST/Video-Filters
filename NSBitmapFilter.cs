using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GrayScaleFilter
{
    static class NSBitmapFilter
    {   //<summary>
        //<para> Applies Filter to Bitmap, returns Bitmap </para>
        //<para>All Made by Normantas Stankevicius</para>
        // Uses System.Common.Drawing Library
        //</summary>

        public static Bitmap ConvertToGrayscale(Bitmap bitmap)
        {
            // Convers the pixels of a BitmapToGray
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    int grayColor = System.Convert.ToInt32(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
                    pixel = Color.FromArgb(pixel.A, grayColor, grayColor, grayColor);
                    bitmap.SetPixel(x, y, pixel);
                }
            return bitmap;
        }
        public static Bitmap ConvertToNeonware(Bitmap bitmap)
        {
            // Convers the pixels of a Bitmap to vaporwave
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixel = bitmap.GetPixel(x, y).ToNeonware();
                    bitmap.SetPixel(x, y, pixel);
                }
            return bitmap;
        }
        private static Color ToNeonware(this Color pixel)
        {
            //<Summary>
            // Converts a pixel of an image to vaporwave colors
            //</summary>
            int grayScale = System.Convert.ToInt32(0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B);
            int epsilon = 7; // Allowed Deviation from the original
            if (Math.Abs(pixel.R - grayScale) <= epsilon && Math.Abs(pixel.G - grayScale) <= epsilon && Math.Abs(pixel.B - grayScale) <= epsilon)
                return pixel;
            else
            {
                // Applies Vaporwave
                // R - Red, G - Green, B - Blue of a color
                // Y - Luma, U - Blue projection, V - Red Projection
                // RGB to YUV
                
                double Y = 0.257 * pixel.R + 0.504 * pixel.G + 0.098 * pixel.B + 16;
                double U = -0.148 * pixel.R - 0.291 * pixel.G + 0.439 * pixel.B + 128;
                double V = 0.439 * pixel.R - 0.368 * pixel.G - 0.071 * pixel.B + 128;

                double yMultiplier = 1.70 + (Math.Log(Y,0.9) / 100);
                double uMultiplier = 1.52 + (Math.Log(U, 0.9) / 100);
                double vMultiplier = 1.52 + (Math.Log(V, 0.9) / 100);
                Y = Math.Round(Y * yMultiplier,2);
                if (Math.Abs(U) > Math.Abs(V))
                    U = Math.Round(U * uMultiplier,2);
                else if (Math.Abs(V) > Math.Abs(U))
                    V = Math.Round(V * uMultiplier, 2);


                // YUV to RGB
                Y -= 16;
                U -= 128;
                V -= 128;
                
                int R = Convert.ToInt32(1.164 * Y + 1.596 * V);
                int G = Convert.ToInt32(1.164 * Y - 0.392 * U - 0.813 * V);
                int B = Convert.ToInt32(1.164 * Y + 2.017 * U);
                double logRed = Math.Round(Math.Log(R, 0.89) / 100, 2);
                double logGreen = 0;
                double logBlue = Math.Round(Math.Log(B, 0.89) / 100, 2);
                double redMultiplier = 1.7 + logRed;
                double greenMultiplier = 0.7 + logGreen;
                double blueMultiplier = 1.6 + logBlue;
                R = Convert.ToInt32(R * redMultiplier);
                G = Convert.ToInt32(G * greenMultiplier);
                B = Convert.ToInt32(B * blueMultiplier);

                // Safety Net
                if (R >= 255)
                    R = 255;
                else if (R < 0)
                    R = 0;

                if (G >= 255)
                    G = 255;
                else if (G < 0)
                    G = 0;

                if (B >= 255)
                    B = 255;
                else if (B < 0)
                    B = 0;

                pixel = Color.FromArgb(pixel.A, R, G, B);
            }
            return pixel;
        }
    }
}
