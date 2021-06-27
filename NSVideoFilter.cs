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
            FFmpegCommunicator.Execute($" -f image2 -framerate 24 -pattern_type glob -i \'{outputImageDir}\\*.png\' -vf format=yuv420p output.mp4");
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
                filterFunction(imageData.ToBitmap()).Save($"{outputImageDir}/{frameCount++}.png");
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
            var rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
            var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            FFMediaToolkit.Graphics.ImageData bitmapData = FFMediaToolkit.Graphics.ImageData.FromPointer(bitLock.Scan0,FFMediaToolkit.Graphics.ImagePixelFormat.Bgr24, bitmap.Size);
            bitmap.UnlockBits(bitLock);
            return bitmapData;
        }
        /* Legacy Functions
         private static void CombineImagesToVid(string fileLocation, int frameCount)
         {
           using Splicer;
            using Splicer.Timeline;
            using Splicer.Renderer;
            using Splicer.WindowsMedia;

             // Uses Splicer
             System.IO.DirectoryInfo directory = new DirectoryInfo(outputImageDir);
             FFMediaToolkit.Decoding.MediaFile mediaFile = FFMediaToolkit.Decoding.MediaFile.Open(fileLocation);
             double timeFps = 1 / mediaFile.Video.Info.AvgFrameRate;
             ITimeline timeline = new DefaultTimeline();
             IGroup group = timeline.AddVideoGroup($"video", mediaFile.Video.Info.AvgFrameRate, 32, mediaFile.Video.Info.FrameSize.Width, mediaFile.Video.Info.FrameSize.Height);
             ITrack videoTrack = group.AddTrack();
             for (int i = 0; i < frameCount; i++)
             {

                 Console.WriteLine($"Adding: image {outputImageDir}\\{i}.jpeg to video");
                 videoTrack.AddImage($"{outputImageDir}\\{i}.jpeg",0,i* timeFps, (i+1)* timeFps);
             }


             // Adds audio
             ITrack audioTrack = timeline.AddAudioGroup().AddTrack();

             // IClip audio = audioTrack.AddAudio(fileLocation, 0, videoTrack.Duration);

             IRenderer renderer = new WindowsMediaRenderer(timeline, outputPath, WindowsMediaProfiles.HighQualityVideo);
             renderer.Render();
             timeline.Dispose();
         }
         private static void SetUpVideoSettings(string fileLocation)
        {
            // Legacy Function
            // Uses FFMediaToolKit
            // Uses MediaToolKit
            FFMediaToolkit.Decoding.MediaFile originalFile = FFMediaToolkit.Decoding.MediaFile.Open(fileLocation);
            var inputFile = new MediaToolkit.Model.MediaFile { Filename = outputPath };
            var outputFile = new MediaToolkit.Model.MediaFile { Filename = $@"{systemDir}outputTranscoded.mp4" };

            var conversionOptions = new ConversionOptions
            {
                VideoFps = Convert.ToInt32(Math.Ceiling(originalFile.Video.Info.AvgFrameRate)),
                MaxVideoDuration = (TimeSpan)originalFile.Video.Info.Duration,
                VideoAspectRatio = VideoAspectRatio.R16_9,
                VideoSize = VideoSize.Ntsc,
            };
            using (var engine = new Engine())
            {
                engine.Convert(inputFile, outputFile, conversionOptions);
            }
        }
         private static void CombineImagesUsingFFMTK(string fileLocation, int frameCount)
        {
            // Not working
            File.Delete(outputPath);
            System.IO.DirectoryInfo directory = new DirectoryInfo(outputImageDir);
            FFMediaToolkit.Decoding.MediaFile mediaFile = FFMediaToolkit.Decoding.MediaFile.Open(fileLocation);
            // You can set there codec, bitrate, frame rate and many other options.
            var settings = new VideoEncoderSettings(width: mediaFile.Video.Info.FrameSize.Width, height: mediaFile.Video.Info.FrameSize.Height, framerate: 30, codec: VideoCodec.H264);
            settings.EncoderPreset = EncoderPreset.Fast;
            settings.CRF = 17;
            using (var video = MediaBuilder.CreateContainer(outputPath).WithVideo(settings).Create())
            {
                for (int i = 0; i < frameCount; i++)
                {
                    Bitmap bitmap = new Bitmap($"{outputImageDir}\\{i}.jpeg");
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size);
                    BitmapData bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                    FFMediaToolkit.Graphics.ImageData bitmapImageData = FFMediaToolkit.Graphics.ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);
                    Console.WriteLine($"Adding: image {outputImageDir}\\{i}.jpeg to video");
                    video.Video.AddFrame(bitmapImageData);
                    bitmap.UnlockBits(bitLock);
                }
            }
        }
         
         */
    }
}
