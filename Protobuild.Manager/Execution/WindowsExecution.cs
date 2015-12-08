using System;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
    public class WindowsExecution : IExecution
    {
		public Process ExecuteConsoleExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
			return this.ExecuteApplicationExecutable(path, arguments, configureStartInfo, configureProcessBeforeStart);
        }

		public Process ExecuteApplicationExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
			startInfo.Arguments = arguments;
            startInfo.WorkingDirectory = new FileInfo(path).Directory.FullName;
			if (configureStartInfo != null)
			{
				configureStartInfo(startInfo);
			}

			var process = new Process();
			process.StartInfo = startInfo;
			if (configureProcessBeforeStart != null)
			{
				configureProcessBeforeStart(process);
			}

			process.Start();
			return process;
        }
    }
}

