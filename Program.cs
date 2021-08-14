using System;
using System.Drawing;
using System.IO;
using System.Reflection;
namespace GrayScaleFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            string systemDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            NSVideoFilter.ConvertToNeonware($"{systemDirectory}video.mp4", NSVideoFilter.GetDirOutputPath());
        }

    }

}
