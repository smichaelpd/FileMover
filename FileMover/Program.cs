using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Timers;

namespace FileMover
{
    internal class Program
    {
        private static Timer copyTimer;
        private static string stagingPath = @"C:\Staging";
        private static string uploadPath = @"C:\Ready";
        private static string fileExtension = "txt";
        private static double interval = 1000;
        private static double minFileAge = 3;

        static void Main(string[] args)
        {
            stagingPath = ConfigurationManager.AppSettings["PathIN"];
            uploadPath = ConfigurationManager.AppSettings["PathOUT"];
            fileExtension = ConfigurationManager.AppSettings["FileExtension"];

            bool getInterval = double.TryParse(ConfigurationManager.AppSettings["IntervalSeconds"], out double waitInterval);
            if (getInterval)
            {
                interval = waitInterval * 1000;
            }

            bool getMinFileAge = double.TryParse(ConfigurationManager.AppSettings["MinAgeSeconds"], out double minAge);
            if (getMinFileAge)
            {
                minFileAge = minAge;
            }

            SetTimer();

            while (1 == 1)
            {
            }
        }

        private static void SetTimer()
        {
            copyTimer = new Timer(interval);
            copyTimer.Elapsed += OnTimedEvent;
            copyTimer.AutoReset = true;
            copyTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            DirectoryInfo info = new DirectoryInfo(stagingPath);
            var files = info.GetFiles("*." + fileExtension)
                .Where(x => DateTime.Now - x.CreationTime > TimeSpan.FromSeconds(minFileAge))
                .OrderBy(p => p.CreationTime);
            if (files.Any())
            {
                if (CopyFile(files.First().FullName, Path.Combine(uploadPath, files.First().Name)))
                {
                    File.Delete(files.First().FullName);
                };
            }
        }

        private static bool CopyFile(string source, string destination)
        {
            File.Copy(source, destination, true);
            if (!File.Exists(destination))
            {
                return false;
            }

            return true;
        }
    }
}
