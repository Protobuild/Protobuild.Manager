using System;
using System.Diagnostics;
using System.IO;

namespace Unearth
{
    public class MacOSExecution : IExecution
    {
        public void ExecuteConsoleExecutable(string path)
        {
            try
            {
                var p = Process.Start("chmod", "u+x '" + path + "'");
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't mark game as executable");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/mono";
            startInfo.Arguments = path;
            startInfo.WorkingDirectory = new FileInfo(path).Directory.FullName;

            Process.Start(startInfo);
        }

        public void ExecuteApplicationExecutable(string path)
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
                throw new Exception("Can't mark game as executable");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "/usr/bin/open";
            startInfo.Arguments = "-a " + path;
            startInfo.WorkingDirectory = new FileInfo(path).Directory.FullName;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            var process = Process.Start(startInfo);
            var output = string.Empty;

            process.OutputDataReceived += (sender, e) => output += e.Data;
            process.ErrorDataReceived += (sender, e) => output += e.Data;

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            if (process.ExitCode == 1)
            {
                throw new Exception("Game executable unable to start");
            }
        }
    }
}

