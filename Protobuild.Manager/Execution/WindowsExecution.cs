using System;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
    public class WindowsExecution : IExecution
    {
        public void ExecuteConsoleExecutable(string path)
        {
            this.ExecuteApplicationExecutable(path);
        }

        public void ExecuteApplicationExecutable(string path)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = path;
            startInfo.WorkingDirectory = new FileInfo(path).Directory.FullName;

            Process.Start(startInfo);
        }
    }
}

