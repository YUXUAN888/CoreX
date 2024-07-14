using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreX.Install
{
    public class ThreadDownloader
    {
        //线程下载器
        private readonly string _downloadUrl;
        private readonly string _savePath;
        private readonly int _threadCount;

        public ThreadDownloader(string downloadUrl, string savePath, int threadCount)
        {
            _downloadUrl = downloadUrl;
            _savePath = savePath;
            _threadCount = threadCount;
        }
        public void StartDownload()
        {
            // 获取文件大小
            long fileSize;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_downloadUrl);
            try
            {
                webRequest.Method = "HEAD";
                using (var webResponse = webRequest.GetResponse())
                {
                    fileSize = long.Parse(webResponse.Headers["Content-Length"]);
                }
            }
            finally
            {
                webRequest.Abort();
            }

            // 计算每个线程下载的字节范围
            long chunkSize = fileSize / _threadCount;
            var threads = new Thread[_threadCount];

            for (int i = 0; i < _threadCount; i++)
            {
                long start = i * chunkSize;
                long end = (i == _threadCount - 1) ? fileSize - 1 : (start + chunkSize - 1);

                threads[i] = new Thread(() => DownloadChunk(start, end));
                threads[i].Start();
            }

            // 等待所有线程完成
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        HttpWebRequest httpWebRequest;
        private void DownloadChunk(long start, long end)
        {
            int retryCount = 20;  // 错误重试次数

            while (retryCount > 0)
            {
                try
                {
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(_downloadUrl);
                    httpWebRequest.Method = "GET";
                    httpWebRequest.AddRange(start, end);

                    using (var webResponse = httpWebRequest.GetResponse())
                    {
                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            using (var fileStream = new FileStream(_savePath, FileMode.Append, FileAccess.Write))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                long totalBytesRead = 0;

                                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;

                                    // 计算并返回下载百分比
                                    double percentage = (double)(start + totalBytesRead) / (end - start + 1) * 100;
                                    Console.WriteLine($"线程下载进度: {percentage:F2}%");
                                }
                            }
                        }
                    }

                    break;  // 下载成功，退出重试
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"下载出错，重试次数: {retryCount}。错误信息: {ex.Message}");
                    retryCount--;
                }
                finally
                {
                    httpWebRequest.Abort();
                }
            }
        }
    }
}
