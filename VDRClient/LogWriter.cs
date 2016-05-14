using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace VDRClient
{
    class LogWriter
    {
        private static Queue<string> queue = new Queue<string>();

        public static void WriteToLog(string line)
        {
            line = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + ": " + line + "\r\n";
            queue.Enqueue(line);
        }

        public static async void WriteLogToFile()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile logFile = await storageFolder.CreateFileAsync("vdrclient_log.txt", CreationCollisionOption.OpenIfExists);
                while (queue.Count > 0)
                {
                    await FileIO.AppendTextAsync(logFile, queue.Dequeue());
                }
            }
            catch { }
        }

        public static async Task<string> ReadLogFile()
        {
            string log = "";
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile logFile = await storageFolder.GetFileAsync("vdrclient_log.txt");
                log = await FileIO.ReadTextAsync(logFile);
            }
            catch { }
            return log;
        }

        public static async Task DeleteLogfile()
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile logFile = await storageFolder.CreateFileAsync("vdrclient_log.txt", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(logFile, "");
            }
            catch { }
        }

        public static async Task<StorageFile> GetLogFile()
        {
            StorageFile logFile = null;
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                logFile = await storageFolder.CreateFileAsync("vdrclient_log.txt", CreationCollisionOption.OpenIfExists);
            }
            catch { }
            return logFile;
        }
    }
}
