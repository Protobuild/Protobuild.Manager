// ====================================================================== //
// This source code is licensed in accordance with the licensing outlined //
// on the main Tychaia website (www.tychaia.com).  Changes to the         //
// license on the website apply retroactively.                            //
// ====================================================================== //
namespace Protobuild.Manager
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading;

    public class GameUpdater
    {
        private readonly string m_ConduitUser;

        private readonly string m_ConduitCertificate;

        private readonly string m_SourceName;

        private readonly string m_GamePath;

        public GameUpdater(
            string conduitUser,
            string conduitCertificate,
            string sourceName,
            string gamePath)
        {
            this.m_ConduitUser = conduitUser;
            this.m_ConduitCertificate = conduitCertificate;
            this.m_SourceName = sourceName;
            this.m_GamePath = gamePath;
        }

        public bool Update(
            Action<string> onStatus,
            Action<string> onFail,
            Action<float, TimeSpan> onProgress)
        {
            /*
            var client = new Phabricator.Conduit.ConduitClient(UrlConfig.CONDUIT);
            client.Certificate = this.m_ConduitCertificate;
            client.User = this.m_ConduitUser;

            onStatus("Checking for updates");

            var result = client.Do<Dictionary<string, object>>("phragment.queryfragments", new
            {
                paths = new[] { this.m_SourceName },
                snapshot = ConfigManager.LoadChannel()
            });

            var entries = (ArrayList)result[this.m_SourceName];

            var toDownload = new Dictionary<string, string>();

            Directory.CreateDirectory(this.m_GamePath);

            Dictionary<string, string> existingHashes;
            try
            {
                existingHashes = this.CalculateHashes();
            }
            catch (IOException ex)
            {
                // Files are locked.
                onFail("Unable to update; is the game currently running?");
                ErrorLog.Log(ex);
                return false;
            }
            
            var newHashes = new Dictionary<string, string>();

            long totalBytes = 0;

            foreach (var entry in entries.Cast<Dictionary<string, object>>())
            {
                var destPath = Path.Combine(this.m_GamePath, (string)entry["path"]);

                if (!File.Exists(destPath))
                {
                    // Doesn't exist, pre-download
                    toDownload.Add((string)entry["path"], (string)entry["uri"]);
                    totalBytes += int.Parse((string)entry["size"], CultureInfo.InvariantCulture.NumberFormat);
                }
                else if (existingHashes[(string)entry["path"]].ToUpperInvariant() != ((string)entry["hash"]).ToUpperInvariant())
                {
                    // Changed, re-download
                    toDownload.Add((string)entry["path"], (string)entry["uri"]);
                    totalBytes += int.Parse((string)entry["size"], CultureInfo.InvariantCulture.NumberFormat);
                }

                newHashes.Add((string)entry["path"], (string)entry["hash"]);
            }

            foreach (var entry in existingHashes)
            {
                if (!newHashes.ContainsKey(entry.Key))
                {
                    var destPath = Path.Combine(this.m_GamePath, entry.Key);

                    // Explicitly delete file from disk
                    try
                    {
                        onStatus("Removing " + entry.Key);

                        File.Delete(destPath);
                    }
                    catch (Exception ex)
                    {
                        // Ignore failure!
                        ErrorLog.Log(ex);
                    }
                }
            }

            var downloader = new ProgressRetryDownloader();
            return downloader.DownloadFiles(
                toDownload,
                this.m_GamePath,
                onStatus,
                onProgress,
                onFail,
                totalBytes);
                */
            return true;
        }

        private Dictionary<string, string> CalculateHashes()
        {
            var files = this.GetRecursiveFiles(this.m_GamePath).ToList();
            var sha1 = new SHA1Cng();
            var result = new Dictionary<string, string>();

            foreach (var fullPath in files)
            {
                var basename = fullPath.Substring(this.m_GamePath.Length);
                basename = basename.Replace('\\', '/');
                basename = basename.Trim('/');

                var stream = new FileStream(fullPath, FileMode.Open);
                var hashBytes = sha1.ComputeHash(stream);
                stream.Dispose();

                var hashString = BitConverter.ToString(hashBytes)
                    .Replace("-", string.Empty);

                result.Add(basename, hashString);
            }

            return result;
        }

        private IEnumerable<string> GetRecursiveFiles(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            foreach (var directory in directoryInfo.GetDirectories())
            {
                foreach (var entry in this.GetRecursiveFiles(directory.FullName))
                {
                    yield return entry;
                }
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                yield return file.FullName;
            }
        }
    }
}

