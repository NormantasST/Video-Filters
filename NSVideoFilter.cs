using FFMediaToolkit.Decoding;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using Color = System.Drawing.Color;
using System;
using System.Collections.Generic;
using FFMediaToolkit.Encoding;
using FFMediaToolkit.Graphics;
using MediaToolkit;
using MediaToolkit.Options;
namespace GrayScaleFilter
{
    static class NSVideoFilter
    {
        private static string systemDir = System.AppDomain.CurrentDomain.BaseDirectory;
        private static string outputImageDir = $"{systemDir}images";
        private static string FFmpegPath = $@"{systemDir}ffmpeg";

        public static void ConvertVideo(string inputPath, string outputPath, Func<Bitmap, Bitmap> filterFuction)
        {
            FFMediaToolkit.FFmpegLoader.FFmpegPath = FFmpegPath;
            //CreateDirectory();
            //ExtractImages(inputPath, filterFuction);
            DeleteOutputFile(outputPath);
            CombineImagesToVid(inputPath);
            //DeleteImageDir();
            Console.WriteLine("-----------------------");
            Console.WriteLine("Filter Has been applied");
        }

        public static void ConvertToNeonware(string inputPath, string outputPath)
            => ConvertVideo(inputPath, outputPath, NSBitmapFilter.ConvertToNeonware);

        public static void ConvertToGrayscale(string inputPath, string outputPath)
            => ConvertVideo(inputPath, outputPath, NSBitmapFilter.ConvertToGrayscale);

        public static string GetDirOutputPath() // For QuickUse
            => $@"{systemDir}output.mp4";

        private static void DeleteOutputFile(string filePath)
        {
            // Deletes Output file if already exists so it can Override it.

            if (File.Exists(filePath))
                File.Delete(filePath);  

        }
        private static void CreateDirectory()
        {
            // Creates directory for saving images

            if (Directory.Exists(outputImageDir) == false)
            {
                Console.WriteLine("Images Directory Added");
                Directory.CreateDirectory(outputImageDir);
            }

            else
            {
                DeleteImageDir();
            }

        }

        private static void DeleteImageDir()
        {
            // Deletes ./images directory

            System.IO.DirectoryInfo di = new DirectoryInfo(outputImageDir);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            Console.WriteLine("Images Directory Emptied");
        }

        private static void CombineImagesToVid(string fileLocation)
        {
            // Uses FFMPEG

            System.IO.DirectoryInfo directory = new DirectoryInfo(outputImageDir);
            FFMediaToolkit.Decoding.MediaFile mediaFile = FFMediaToolkit.Decoding.MediaFile.Open(fileLocation);
            FFmpegCommunicator.Start();
            Console.WriteLine($"Adding: images to video");
            FFmpegCommunicator.Execute($"ffmpeg -r 1/3 -start_number 1 -i \'{outputImageDir}\\img%03d.png -c:v libx264 -vf fps=25 -pix_fmt yuv420p out.mp4");
            //FFmpegCommunicator.Execute($" -f image2 -framerate 24 -pattern_type glob -i \'{outputImageDir}\\*.png\' -vf format=yuv420p output.mp4");
            FFmpegCommunicator.End();
        }

        private static int ExtractImages(string fileLocation, Func<Bitmap,Bitmap> filterFunction)
        {
            // Extracts images to outputImageDir

            int frameCount = 1;
            FFMediaToolkit.Decoding.MediaFile originalFile = FFMediaToolkit.Decoding.MediaFile.Open(fileLocation);
            Console.WriteLine($"Duration: {originalFile.Info.Duration}, FrameCount: {originalFile.Video.Info.NumberOfFrames}, AvgFrameRate: {originalFile.Video.Info.AvgFrameRate}");

            while (originalFile.Video.TryGetNextFrame(out FFMediaToolkit.Graphics.ImageData imageData))
            {
                filterFunction(imageData.ToBitmap()).Save($"{outputImageDir}/img{frameCount++.ToString("000")}.png");
                Console.WriteLine($"Exported Image Count: {frameCount - 1}");
            }
            return frameCount;
        }

        private static unsafe Bitmap ToBitmap(this FFMediaToolkit.Graphics.ImageData bitmap)
        {
            // Converts ImageData to Bitmap
            // Uses FFMediaToolKit for ImageData
            // ImageData -> Bitmap (unsafe)

            fixed (byte* p = bitmap.Data)
            {
                return new Bitmap(bitmap.ImageSize.Width, bitmap.ImageSize.Height, bitmap.Stride, PixelFormat.Format24bppRgb, new System.IntPtr(p));
            }
        }

        private static FFMediaToolkit.Graphics.ImageData ToImageData(this Bitmap bitmap)
        {
            // Converts Bitmap to ImageData
            // Uses FFMediaToolKit for ImageData
            // Bitmap -> ImageData (safe)

            var rect    = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
            var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            FFMediaToolkit.Graphics.ImageData bitmapData = FFMediaToolkit.Graphics.ImageData.FromPointer(bitLock.Scan0,FFMediaToolkit.Graphics.ImagePixelFormat.Bgr24, bitmap.Size);
            bitmap.UnlockBits(bitLock);

            return bitmapData;
        }

    }

}
