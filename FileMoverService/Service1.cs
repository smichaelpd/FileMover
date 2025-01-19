using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;

namespace FileMoverService
{
    public partial class FileMoverService : ServiceBase
    {
        private static System.Timers.Timer timer;
        private static string input_dir = @"D:\Shares\Sales_Export\Staging";
        private static string output_dir = @"D:\Shares\Sales_Export";
        //private static string input_dir = @"C:\Staging";
        //private static string output_dir = @"C:\Export";
        private static string fileExtension = "*.ord";
        private static double interval = 1.5;
        private static double minFileAge = 3;
        List<string> copyInProgress = new List<string>();

        public FileMoverService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!Directory.Exists(input_dir))
            {
                Directory.CreateDirectory(input_dir);
            }
            if (!Directory.Exists(output_dir))
            {
                Directory.CreateDirectory(output_dir);
            }

            timer = new System.Timers.Timer(interval * 1000);
            timer.Elapsed += (s, e) => CopyFile();
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Enabled = false;
            timer.Dispose();
        }

        private void CopyFile()
        {
            DirectoryInfo info = new DirectoryInfo(input_dir);
            FileInfo file = info.GetFiles(fileExtension, SearchOption.TopDirectoryOnly)
                .Where(x => (DateTime.Now - x.LastWriteTime > TimeSpan.FromSeconds(minFileAge)) 
                            && !copyInProgress.Contains(x.Name)).ToList()
                .OrderBy(p => p.CreationTime).FirstOrDefault();

            if (file != null)
            {
                copyInProgress.Add(file.Name);
                string destFile = Path.Combine(output_dir, file.Name);
                try
                {
                    File.Copy(file.FullName, destFile, true);

                    if (File.Exists(destFile))
                    {
                        File.Delete(file.FullName);
                    }
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("FileMoverService", ex.Message, EventLogEntryType.Error);
                }
                finally
                {
                    copyInProgress.Remove(file.Name);
                }
            }
        }
    }
}
