using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Protobuild.Manager
{
    public class MonoDevelopLinuxIDEControl : LinuxIDEControl
    {
		private readonly IExecution _execution;

        private readonly RuntimeServer _runtimeServer;

		public MonoDevelopLinuxIDEControl(RuntimeServer runtimeServer, IExecution execution, IProcessLog processLog) : base(runtimeServer, execution, processLog)
        {
			_execution = execution;
            _runtimeServer = runtimeServer;
        }

        protected override async Task OpenIDE(string modulePath, string moduleName, string targetPlatform)
        {
            var mdpid = _runtimeServer.Get<int?>("monodeveloppid");

            if (mdpid != null)
            {
                // We just don't do anything for now, but later on the extension should
                // provide a socket or some other way of communicating with the MonoDevelop
                // process so we can signal it to open the solution.
                return;
            }

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

