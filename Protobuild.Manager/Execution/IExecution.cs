using System;
using System.Diagnostics;

namespace Protobuild.Manager
{
    public interface IExecution
    {
		Process ExecuteConsoleExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo = null, Action<Process> configureProcessBeforeStart = null);

		Process ExecuteApplicationExecutable(string path, string arguments, Action<ProcessStartInfo> configureStartInfo = null, Action<Process> configureProcessBeforeStart = null);
    }
}

