using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Protobuild.Manager
{
    public class MonoDevelopLinuxIDEControl : LinuxIDEControl
    {
		private readonly IExecution _execution;

		public MonoDevelopLinuxIDEControl(RuntimeServer runtimeServer, IExecution execution, IProcessLog processLog) : base(runtimeServer, execution, processLog)
        {
			_execution = execution;
        }

        protected override async Task OpenIDE(string modulePath, string moduleName, string targetPlatform)
        {
			var process = _execution.ExecuteConsoleExecutable(
				"/usr/bin/monodevelop",
				"\"" + Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln") + "\"",
				x =>
				{
					x.WorkingDirectory = modulePath;
				});
            await process.WaitForExitAsync();
        }
    }
}

