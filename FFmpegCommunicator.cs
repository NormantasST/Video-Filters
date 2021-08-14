using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GrayScaleFilter
{

    static class FFmpegCommunicator
    {
        private static string systemDir = System.AppDomain.CurrentDomain.BaseDirectory;
        public static string fileName { get; set; } = "ffmpeg.exe";
        public static string ffmpegPath { get; set; } = $@"{systemDir}ffmpeg\{fileName}";
        private static ProcessStartInfo startInfo;
        private static bool hasStarted = false;
        private static Process exeProcess;

        public static void Start()
        {
            // Starts Communication with FFMPEG

            startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = fileName;
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            hasStarted = true;
            exeProcess = new Process();
        }

        public static void End()
        {
            // Ends communicationwith FFmpeg

            exeProcess.Dispose();
            hasStarted = false;
        }

        public static void Execute(string command)
        {
            // Sends a command to FFMPEG

            startInfo.Arguments = command;
            exeProcess = Process.Start(startInfo);
            exeProcess.WaitForExit();  
        }

    }

}
