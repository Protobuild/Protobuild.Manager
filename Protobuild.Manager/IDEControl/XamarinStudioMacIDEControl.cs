using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
	public class XamarinStudioMacIDEControl : LinuxIDEControl
    {
		public XamarinStudioMacIDEControl(RuntimeServer runtimeServer, IExecution execution, IProcessLog processLog) : base(runtimeServer, execution, processLog)
	    {
	    }

	    protected override async Task OpenIDE(string modulePath, string moduleName, string targetPlatform)
		{
			// NOTE: We don't use IExecution here because we need to use the -n flag to always start a new instance.

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/open";
            startInfo.Arguments = "-n \"/Applications/Xamarin Studio.app\" --args \"" + Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln") + "\"";
            startInfo.WorkingDirectory = modulePath;

            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}

