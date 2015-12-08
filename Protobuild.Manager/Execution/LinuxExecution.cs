using System;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
    public class LinuxExecution : IExecution
    {
		public Process ExecuteConsoleExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
			return this.ExecuteApplicationExecutable(path, arguments, configureStartInfo, configureProcessBeforeStart);
        }

		public Process ExecuteApplicationExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
            // If we are running Linux, we need to mark the file as executable.
            // This allows the server to run as a seperate process.
            try
            {
                var p = Process.Start("chmod", "u+x '" + path + "'");
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't mark application as executable");
            }

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

