using System;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
    public class LinuxExecution : IExecution
    {
        public void ExecuteConsoleExecutable(string path)
        {
            this.ExecuteApplicationExecutable(path);
        }

        public void ExecuteApplicationExecutable(string path)
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
                throw new Exception("Can't mark game as executable");
            }

            var startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.WorkingDirectory = new FileInfo(path).Directory.FullName;

            Process.Start(startInfo);
        }
    }
}

