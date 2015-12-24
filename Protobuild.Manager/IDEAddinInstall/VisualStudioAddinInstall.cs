#if PLATFORM_WINDOWS

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class VisualStudioAddinInstall : IIDEAddinInstall
    {
        private readonly IExecution _execution;
        private readonly IAddinPackageDownload _addinPackageDownload;
        private readonly IConfigManager _configManager;
        private readonly IProcessLog _processLog;
        private readonly IBrandingEngine _brandingEngine;

        private bool _didGacInstall;

        public VisualStudioAddinInstall(
            IExecution execution,
            IAddinPackageDownload addinPackageDownload,
            IConfigManager configManager,
            IProcessLog processLog,
            IBrandingEngine brandingEngine)
        {
            _execution = execution;
            _addinPackageDownload = addinPackageDownload;
            _configManager = configManager;
            _processLog = processLog;
            _brandingEngine = brandingEngine;
        }

        public async Task InstallIfNeeded(bool force)
        {
            var vsVersions = new[]
            {
                "10.0",
                "11.0",
                "12.0",
                "14.0"
            };

            var scheduled = new List<Func<string, Task>>();

            foreach (var version in vsVersions)
            {
                if (!force)
                {
                    if (File.Exists(Path.Combine(_configManager.GetBasePath(), "vs" + version + ".lock")))
                    {
                        _processLog.WriteInfo("The IDE add-in is already installed in Visual Studio " + version + ", and force is not true.  Skipping.");
                        continue;
                    }
                }

                var versionCopy = version;
                var installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "Microsoft Visual Studio " + version);
                var vsixInstallerPath = Path.Combine(installPath,
                    "Common7",
                    "IDE",
                    "VSIXInstaller.exe");
                    
                if (Directory.Exists(installPath))
                {
                    if (File.Exists(vsixInstallerPath) && version != "10.0" && version != "11.0" /* these two versions don't support extensions in all editions */)
                    {
                        _processLog.WriteInfo("Will perform VSIX-based installation for Visual Studio " + version + ".");
                        scheduled.Add(async root => await PerformVSIXInstall(root, versionCopy, vsixInstallerPath, force));
                    }
                    else
                    {
                        _processLog.WriteInfo("Will perform GAC-based installation for Visual Studio " + version + ".");
                        scheduled.Add(async root => await PerformGACInstall(root, versionCopy, force));
                    }
                }
                else
                {
                    _processLog.WriteInfo("Visual Studio " + version + " is not installed (checked for existance of " + installPath + ").");
                }
            }

            if (scheduled.Count == 0)
            {
                _processLog.WriteInfo("No versions of Visual Studio require add-in installation.");
                return;
            }

            _processLog.WriteInfo("Downloading latest IDE add-in for Visual Studio...");
            var packageRoot = await _addinPackageDownload.GetPackageRoot(_brandingEngine.VisualStudioAddinPackage);
            if (packageRoot == null)
            {
                _processLog.WriteError("Unable to download latest IDE add-in for Visual Studio!");
                return;
            }

            foreach (var task in scheduled)
            {
                await task(packageRoot);
            }
        }

        private async Task PerformVSIXInstall(string packageRoot, string version, string vsix, bool force)
        {
            _processLog.WriteInfo("Starting VSIX installation for Visual Studio " + vsix + "...");

            var process = _execution.ExecuteConsoleExecutable(
                vsix,
                "/q \"" + Path.Combine(packageRoot, "VSIX", "Protobuild.IDE.VisualStudio.vsix") + "\"",
                x =>
                {
                    x.UseShellExecute = false;
                    x.CreateNoWindow = true;
                    x.RedirectStandardError = true;
                    x.RedirectStandardOutput = true;
                },
                _processLog.PrepareForAttachToProcess);
            _processLog.AttachToProcess(process);
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _processLog.WriteError("VSIX installer returned a non-zero exit code while installing for Visual Studio " + version + ", falling back to GAC install!");
                await PerformGACInstall(packageRoot, version, force);
                return;
            }

            _processLog.WriteInfo("Execution of VSIX installer completed successfully for Visual Studio " + version + ", marking as installed.");

            using (var writer = new StreamWriter(Path.Combine(_configManager.GetBasePath(), "vs" + version + ".lock")))
            {
                writer.Write("addin installed");
            }
        }

        private async Task PerformGACInstall(string packageRoot, string version, bool force)
        {
            if (_didGacInstall)
            {
                _processLog.WriteInfo("Already did GAC installation this session, skipping GAC installation for Visual Studio " + version + ".");
                return;
            }

            try
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(
                    Path.Combine(packageRoot,
                        "NoVSIX",
                        "Protobuild.IDE.VisualStudio.Wizard.dll"));
                _didGacInstall = true;

                _processLog.WriteInfo("GAC installation completed successfully for Visual Studio " + version + ", marking as installed.");

                using (var writer = new StreamWriter(Path.Combine(_configManager.GetBasePath(), "vs" + version + ".lock")))
                {
                    writer.Write("addin installed");
                }
            }
            catch (Exception ex)
            {
                _processLog.WriteError("Got an exception while performing GAC install for Visual Studio " + version + ": " + ex.Message);

                if (force)
                {
                    throw;
                }
            }
        }
    }
}

#endif