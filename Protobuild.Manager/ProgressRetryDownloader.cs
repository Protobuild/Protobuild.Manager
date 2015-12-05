using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;

namespace Protobuild.Manager
{
    internal class ProgressRetryDownloader
    {
        private readonly IErrorLog _errorLog;

        public ProgressRetryDownloader(IErrorLog errorLog)
        {
            _errorLog = errorLog;
        }

        public bool DownloadFiles(
            Dictionary<string, string> toDownload,
            string destinationFolder,
            Action<string> onStatus,
            Action<float, TimeSpan> onProgress,
            Action<string> onFail,
            long totalBytes)
        {
            var completed = new List<string>();
            var failed = new List<string>();

            var start = DateTime.Now;

            onStatus("Downloading (" + toDownload.Count + " to go)<br/><br/>");

            var bytesReceived = 0L;

            foreach (var kv in toDownload)
            {
                var remaining = toDownload.Count - completed.Count;
                onStatus("Downloading (" + remaining + " to go)<br/><br/>" + kv.Key);

                onProgress(
                    (float)(((double)bytesReceived / (double)totalBytes)),
                    this.EstimateTimeRemaining(start, bytesReceived, totalBytes));

                var webClient = new WebClient();

                var destPath = Path.Combine(destinationFolder, kv.Key);

                var folderInfo = new FileInfo(destPath).DirectoryName;
                Directory.CreateDirectory(folderInfo);

                var tempBytesReceived = 0L;

                using (var writer = new BinaryWriter(new FileStream(destPath, FileMode.Create)))
                {
                    var passed = false;
                    while (!passed)
                    {
                        Exception error = null;
                        var completedFile = false;

                        DownloadProgressChangedEventHandler downloadProgressChanged = null;
                        DownloadDataCompletedEventHandler downloadDataCompleted = null;

                        downloadProgressChanged = (sender, e) => 
                        {
                            tempBytesReceived = e.BytesReceived;
                            onProgress(
                                (float)(((double)(bytesReceived + tempBytesReceived) / (double)totalBytes)),
                                this.EstimateTimeRemaining(start, bytesReceived + tempBytesReceived, totalBytes));
                        };

                        downloadDataCompleted = (sender, e) => 
                        {
                            if (e.Error != null)
                            {
                                error = e.Error;
                                passed = false;
                            }
                            else
                            {
                                writer.Write(e.Result);
                                passed = true;
                            }

                            completedFile = true;

                            webClient.DownloadProgressChanged -= downloadProgressChanged;
                            webClient.DownloadDataCompleted -= downloadDataCompleted;
                        };

                        webClient.DownloadProgressChanged += downloadProgressChanged;
                        webClient.DownloadDataCompleted += downloadDataCompleted;

                        webClient.DownloadDataAsync(new Uri(kv.Value));

                        while (!completedFile)
                        {
                            Thread.Sleep(50);
                            onProgress(
                                (float)(((double)(bytesReceived + tempBytesReceived) / (double)totalBytes)),
                                this.EstimateTimeRemaining(start, bytesReceived + tempBytesReceived, totalBytes));
                        }

                        if (error != null)
                        {
                            _errorLog.Log(error);

                            var webExceptionError = error as WebException;
                            if (webExceptionError == null)
                            {
                                throw error;
                            }
                            else if (webExceptionError.Status == WebExceptionStatus.ReceiveFailure)
                            {
                                // Temporary failure, try again.
                                continue;
                            }
                            else
                            {
                                onFail("Failed to download " + kv.Key + " (" + webExceptionError.Status + ")");
                                return false;
                            }
                        }
                    }

                    bytesReceived += writer.BaseStream.Position;

                    onProgress(
                        (float)(((double)bytesReceived / (double)totalBytes)),
                        this.EstimateTimeRemaining(start, bytesReceived, totalBytes));

                    writer.Close();
                }

                completed.Add(kv.Key);
            }

            return true;
        }

        private TimeSpan EstimateTimeRemaining(DateTime start, long current, long total)
        {
            var now = DateTime.Now;
            var taken = now - start;

            var estimatedTicks = taken.Ticks * ((double)total / (double)current);
            var remainingTicks = estimatedTicks - taken.Ticks;
            return new TimeSpan((long)remainingTicks);
        }
    }
}

