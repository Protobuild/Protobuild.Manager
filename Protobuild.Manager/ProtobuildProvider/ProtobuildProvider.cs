using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
                    var branchJson = await client.DownloadStringTaskAsync("https://api.github.com/repos/Protobuild/Protobuild/branches/master");
                    var serializer = new JavaScriptSerializer();
                    var branchInfo = serializer.Deserialize<Dictionary<string, object>>(branchJson);
                    var commitInfo = (Dictionary<string, object>) branchInfo["commit"];
					targetHash = (string)commitInfo["sha"];

					_processLog.WriteInfo("Current version of Protobuild: " + cachedHash);
					_processLog.WriteInfo("Latest version of Protobuild: " + targetHash);

                    if (cachedHash != targetHash)
                    {
                        downloadNewVersion = true;
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        // Probably rate limited by GitHub, try to use "git ls-remote" instead.
                        try
                        {
                            var heads = RunGitAndCapture(
                                Environment.CurrentDirectory,
                                null,
                                "ls-remote --heads " + new Uri("https://www.github.com/Protobuild/Protobuild"));

                            var lines = heads.Split(new string[] {"\r\n", "\n", "\r"},
                                StringSplitOptions.RemoveEmptyEntries);

                            foreach (var line in lines)
                            {
                                var sourceEntryComponents = line.Split('\t');
                                if (sourceEntryComponents.Length >= 2)
                                {
                                    var branchName = sourceEntryComponents[1].Trim();

                                    if (branchName.StartsWith("refs/heads/"))
                                    {
                                        branchName = branchName.Substring("refs/heads/".Length);
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    if (string.Equals("master", branchName, StringComparison.InvariantCulture))
                                    {
										// This is a match and we've found our Git hash to use.
										targetHash = sourceEntryComponents[0].Trim();

					                    string cachedHash;
					                    using (var reader = new StreamReader(cachedHashPath))
					                    {
					                        cachedHash = reader.ReadToEnd().Trim();
					                    }

										_processLog.WriteInfo("Current version of Protobuild: " + cachedHash);
										_processLog.WriteInfo("Latest version of Protobuild: " + targetHash);

										if (cachedHash != targetHash)
										{
											downloadNewVersion = true;
										}

                                        break;
                                    }
                                }
                            }

                            if (targetHash == null)
							{
								_processLog.WriteError("Unable to find master branch via Git strategy for update check");
                            }
						}
						catch (Exception ex2)
						{
							// Don't update, use the existing version.
							_processLog.WriteError("Unable to check for newer Protobuild: " + ex2);
						}
                    }
                    else
					{
                        // Unknown web exception.
						_processLog.WriteError("Unable to check for newer Protobuild: " + ex);
                    }
                }
                catch (Exception ex)
                {
					// Don't update, use the existing version.
                    _processLog.WriteError("Unable to check for newer Protobuild: " + ex);
                }
            }

            if (downloadNewVersion)
            {
                _processLog.WriteInfo("Downloading latest version...");
                updateStatus("Downloading latest version...");
                await client.DownloadFileTaskAsync("https://github.com/Protobuild/Protobuild/raw/master/Protobuild.exe", cachedPath);

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

		public string RunGitAndCapture(string workingDirectory, string folder, string str)
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = GetCachedGitPath(),
				Arguments = str,
				WorkingDirectory = folder == null ? workingDirectory : Path.Combine(workingDirectory, folder),
				RedirectStandardOutput = true,
				RedirectStandardInput = true,
				CreateNoWindow = true,
				UseShellExecute = false,
			};

			var suffix = folder == null ? "" : " (" + folder + ")";
			_processLog.WriteInfo("Executing: git " + str + suffix);

			var process = Process.Start(processStartInfo);
			if (process == null)
			{
				throw new InvalidOperationException("Unable to execute Git!");
			}

			process.StandardInput.Close();
			var result = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			if (process.ExitCode != 0)
			{
				throw new InvalidOperationException("Got an unexpected exit code of " + process.ExitCode + " from Git");
			}

			return result;
		}

		private static string _cachedGitPath;

		private static string GetCachedGitPath()
		{
			if (_cachedGitPath == null)
			{
				_cachedGitPath = FindGitOnSystemPath();
			}

			return _cachedGitPath;
		}

		private static string FindGitOnSystemPath()
		{
			if (Path.DirectorySeparatorChar != '/')
			{
				// We're on Windows.  We split the environment PATH to see if we
				// can find Git, then we check standard directories (like
				// C:\Program Files (x86)\Git) etc.
				var pathEnv = Environment.GetEnvironmentVariable("PATH");
				var paths = new string[0];
				if (pathEnv != null)
				{
					paths = pathEnv.Split(';');
				}

				var standardPaths = new List<string>
				{
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "cmd"),
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Git", "bin"),
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "cmd"),
					Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Git", "bin"),
				};

				// Add standard paths that GitHub for Windows uses.  Because the file
				// contains a hash, or some other mutable component, we need to search for
				// the PortableGit path.
				var github = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"GitHub");
				if (Directory.Exists(github))
				{
					foreach (var subfolder in new DirectoryInfo(github).GetDirectories())
					{
						if (subfolder.Name.StartsWith("PortableGit_"))
						{
							standardPaths.Add(Path.Combine(subfolder.FullName, "cmd"));
						}
					}
				}

				var filenames = new[] { "git.exe", "git.bat", "git.cmd" };
				foreach (var path in paths.Concat(standardPaths))
				{
					foreach (var filename in filenames)
					{
						if (File.Exists(Path.Combine(path, filename)))
						{
							// We found Git.
							return Path.Combine(path, filename);
						}
					}
				}

				Console.Error.WriteLine(
					"WARNING: Unable to find Git on your PATH, or any standard " +
					"locations.  Have you installed Git on this system?");
				return "git";
			}

			// For UNIX systems, Git should always be on the PATH.
			return "git";
		}
    }
}

