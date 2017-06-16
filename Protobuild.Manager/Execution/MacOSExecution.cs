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

            string mono = null;
            var basePaths = Environment.GetEnvironmentVariable("PATH");
            foreach (var basePath in basePaths.Split(':'))
            {
                var monoPath = Path.Combine(basePath, "mono");
                if (File.Exists(monoPath))
                {
                    mono = monoPath;
                    break;
                }
            }
            if (mono == null)
			{
				var monoPaths = new[]
                {
					"/usr/bin/mono",
					"/usr/local/bin/mono",
					"/Library/Frameworks/Mono.framework/Versions/Current/Commands/mono"
				};
                foreach (var monoPath in monoPaths)
                {
                    if (File.Exists(monoPath))
                    {
                        mono = monoPath;
                        break;
                    }
                }
            }

            if (mono == null)
            {
                throw new InvalidOperationException("Unable to locate Mono! Make sure it is on your PATH.");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = mono;
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

