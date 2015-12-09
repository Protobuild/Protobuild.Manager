using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Protobuild.Manager
{
    public class MonoDevelopLinuxIDEControl : LinuxIDEControl
    {
        public MonoDevelopLinuxIDEControl(RuntimeServer runtimeServer) : base(runtimeServer)
        {
        }

        protected override async Task OpenIDE(string modulePath, string moduleName, string targetPlatform)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/monodevelop";
            startInfo.Arguments = "\"" + Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln") + "\"";
            startInfo.WorkingDirectory = modulePath;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}

