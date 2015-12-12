using System;
using System.Diagnostics;

namespace Protobuild.Manager
{
	public class RuntimeServerProcessLog : IProcessLog
	{
		private readonly RuntimeServer _runtimeServer;

		private object _lock;

		private int _lineCount;

		public RuntimeServerProcessLog(RuntimeServer runtimeServer)
		{
			_runtimeServer = runtimeServer;
			_lock = new object();
		}

		public void AttachToProcess(Process process)
		{
			process.OutputDataReceived += (sender, args) =>
			{
				var line = args.Data;

				Console.WriteLine(args.Data);

				_runtimeServer.Set("processLogLine" + _lineCount + "Text", line);
				_runtimeServer.Set("processLogLine" + _lineCount + "Color", "#000");
				_lineCount++;
				_runtimeServer.Set("processLogLineCount", _lineCount);
			};
			process.ErrorDataReceived += (sender, args) =>
			{
				var line = args.Data;

				Console.Error.WriteLine(args.Data);

				_runtimeServer.Set("processLogLine" + _lineCount + "Text", line);
				_runtimeServer.Set("processLogLine" + _lineCount + "Color", "#000");
				_lineCount++;
				_runtimeServer.Set("processLogLineCount", _lineCount);
			};
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}
	}
}

