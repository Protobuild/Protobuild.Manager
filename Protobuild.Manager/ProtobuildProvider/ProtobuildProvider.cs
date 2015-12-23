using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class ProtobuildProvider : IProtobuildProvider
    {
        private readonly IConfigManager _configManager;
        private readonly IProcessLog _processLog;
        private readonly IBrandingEngine _brandingEngine;

        public ProtobuildProvider(
            IConfigManager configManager,
            IProcessLog processLog,
            IBrandingEngine brandingEngine)
        {
            _configManager = configManager;
            _processLog = processLog;
            _brandingEngine = brandingEngine;
        }

        public async Task<string> GetProtobuild(Action<string> updateStatus)
        {
            _processLog.WriteInfo("Obtaining a copy of Protobuild...");

            var client = new WebClient();
            client.Headers.Add("User-Agent", "Protobuild Manager/1.0.0");

            var downloadNewVersion = false;
            string targetHash = null;
            var cachedPath = Path.Combine(_configManager.GetBasePath(), "Protobuild.exe");
            var cachedHashPath = Path.Combine(_configManager.GetBasePath(), "Protobuild.exe.hash");
            if (!File.Exists(cachedPath))
            {
                downloadNewVersion = true;
            }
            else if (_brandingEngine.ProtobuildUpdatePolicy != ProtobuildUpdatePolicy.Never)
            {
                try
                {
                    string cachedHash;
                    using (var reader = new StreamReader(cachedHashPath))
                    {
                        cachedHash = reader.ReadToEnd().Trim();
                    }

                    _processLog.WriteInfo("Checking to see if the cached version of Protobuild is up-to-date...");
                    updateStatus("Checking for new versions...");

                    // Try to check with GitHub to see if the master branches points somewhere else.
                    var branchJson = await client.DownloadStringTaskAsync("https://api.github.com/repos/hach-que/Protobuild/branches/master");
                    var serializer = new JavaScriptSerializer();
                    var branchInfo = serializer.Deserialize<Dictionary<string, object>>(branchJson);
                    var commitInfo = (Dictionary<string, object>) branchInfo["commit"];
                    targetHash = (string) commitInfo["sha"];

                    if (cachedHash != targetHash)
                    {
                        downloadNewVersion = true;
                    }
                }
                catch (Exception)
                {
                    // Don't update, use the existing version.
                }
            }

            if (downloadNewVersion)
            {
                _processLog.WriteInfo("Downloading latest version...");
                updateStatus("Downloading latest version...");
                await client.DownloadFileTaskAsync("http://protobuild.org/get", cachedPath);

                _processLog.WriteInfo("Download complete.");

                using (var writer = new StreamWriter(cachedHashPath))
                {
                    writer.Write(targetHash);
                }
            }
            else
            {
                _processLog.WriteInfo("Using version of Protobuild that is in the cache.");
            }

            return cachedPath;
        }
    }
}

