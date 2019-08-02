using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Miyop.IoT.Common
{
    public static class TaskHelper
    {
        public static async Task WithTimeoutAfterStart(Func<CancellationToken, Task> operation, TimeSpan timeout)
        {
            var source = new CancellationTokenSource();
            var task = operation(source.Token);
            //After task starts timeout begin to tick
            source.CancelAfter(timeout);
            await task;
        }

        public static async Task CancelTaskAfterTimeout(Func<CancellationToken, Task> operation, TimeSpan timeout)
        {
            var source = new CancellationTokenSource();
            var task = operation(source.Token);
            //After task starts timeout begin to tick
            source.CancelAfter(timeout);
            await task;
        }
    }

    public class Logging
    {
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public static async Task WriteInfoLog(string strFormat, params string[] strParams)
        {
            await WriteLog("INFO", strFormat, strParams);
        }

        public static async Task WriteErrorLog(string strFormat, params string[] strParams)
        {
            await WriteLog("ERROR", strFormat, strParams);
        }

        private static async Task WriteLog(string strLog, string strFormat, params string[] strParams)
        {
            DateTime dtNow;
            string strLogFile;
            string strLogData;
            StorageFolder storageFolder;
            StorageFile storageFile;

            await _semaphoreSlim.WaitAsync();
            try
            {
                dtNow = DateTime.Now;
                strLogFile = string.Format("{0}_{1}.Log", "LOG", dtNow.ToString("yyyyMMdd"));
                strLogData = $"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ff")} {strLog} - {string.Format(strFormat, strParams)}";

                Debug.WriteLine(strLogData);

                storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                storageFile = await storageFolder.CreateFileAsync(strLogFile, CreationCollisionOption.OpenIfExists);

                using (StreamWriter writer = new StreamWriter(await storageFile.OpenStreamForWriteAsync()))
                {
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    await writer.WriteLineAsync(strLogData);
                    writer.Flush();
                }

            }
            catch (Exception eException)
            {
                Debug.WriteLine(string.Format("Error: WriteSystemLog() {0}", eException.Message));
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}