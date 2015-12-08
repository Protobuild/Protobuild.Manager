using System;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
    public class MacOSExecution : IExecution
    {
		public Process ExecuteConsoleExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
            try
            {
                var p = Process.Start("chmod", "u+x '" + path + "'");
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't mark console program as executable");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/mono";
            startInfo.Arguments = path + " " + arguments;
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

		public Process ExecuteApplicationExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo, Action<Process> configureProcessBeforeStart)
        {
            var info = new FileInfo(path);
            var name = info.Name.Substring(0, info.Name.Length - info.Extension.Length);

            try
            {
                var p = Process.Start("chmod", "u+x '" + path + "/Contents/MacOS/" + name + "'");
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't mark application as executable");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/open";
            startInfo.Arguments = "-a " + path + " --args " + arguments;
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

