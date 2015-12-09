using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Protobuild.Manager
{
    public class MonoDevelopLinuxIDEControl : IIDEControl
    {
        private readonly RuntimeServer _runtimeServer;

        public MonoDevelopLinuxIDEControl(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            Process process;

            if (isProtobuild)
            {
                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                _runtimeServer.Set("status", "Synchronising for " + oldPlatformOnFail + " platform...");
                process = Process.Start(new ProcessStartInfo(protobuild, "--sync " + oldPlatformOnFail)
                    {
                        WorkingDirectory = modulePath,
                        UseShellExecute = false
                    });
                if (process == null)
                {
                    throw new InvalidOperationException("can't sync");
                }
                await process.WaitForExitAsync();

                _runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
                process = Process.Start(new ProcessStartInfo(protobuild, "--generate " + targetPlatform)
                    {
                        WorkingDirectory = modulePath,
                        UseShellExecute = false
                    });
                if (process == null)
                {
                    throw new InvalidOperationException("can't generate");
                }
                await process.WaitForExitAsync();
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/monodevelop";
            startInfo.Arguments = "\"" + Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln") + "\"";
            startInfo.WorkingDirectory = modulePath;

            process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}

