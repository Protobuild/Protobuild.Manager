using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Protobuild.Manager
{
	public class RuntimeServerProcessLog : IProcessLog
	{
		private readonly RuntimeServer _runtimeServer;

		private object _lock;

		private int _lineCount;

        private object _lineLock = new object();

		public RuntimeServerProcessLog(RuntimeServer runtimeServer)
		{
			_runtimeServer = runtimeServer;
			_lock = new object();
		}

		public void PrepareForAttachToProcess(Process process)
		{
			process.OutputDataReceived += (sender, args) =>
			{
				var line = args.Data;

			    if (string.IsNullOrWhiteSpace(line))
			    {
			        return;
			    }

			    lock (_lineLock)
			    {
                    _runtimeServer.Set("processLogLine" + _lineCount + "Text", line.TrimEnd());
			        _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#FFF");
			        _lineCount++;
			        _runtimeServer.Set("processLogLineCount", _lineCount);
			    }
			};
			process.ErrorDataReceived += (sender, args) =>
			{
				var line = args.Data;

                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

			    lock (_lineLock)
			    {
                    _runtimeServer.Set("processLogLine" + _lineCount + "Text", line.TrimEnd());
			        _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#FCC");
			        _lineCount++;
			        _runtimeServer.Set("processLogLineCount", _lineCount);
			    }
			};
		    process.Exited += (sender, args) =>
            {
                lock (_lineLock)
                {
                    _runtimeServer.Set("processLogLine" + _lineCount + "Text",
                        "[exit] [" + process.Id + "] " + process.StartInfo.FileName + " " + process.StartInfo.Arguments +
                        " exited with exit code " + process.ExitCode);
                    if (process.ExitCode == 0)
                    {
                        _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#0F0");
                    }
                    else
                    {
                        _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#F00");
                    }
                    _lineCount++;
                    _runtimeServer.Set("processLogLineCount", _lineCount);
                }
            };
		    process.EnableRaisingEvents = true;
        }

	    public void AttachToProcess(Process process)
        {
	        lock (_lineLock)
	        {
	            _runtimeServer.Set("processLogLine" + _lineCount + "Text",
	                "[start] [" + process.Id + "] " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);
	            _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#FF0");
	            _lineCount++;
	            _runtimeServer.Set("processLogLineCount", _lineCount);
	        }

	        process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

	    public void WriteInfo(string message)
        {
            lock (_lineLock)
            {
                _runtimeServer.Set("processLogLine" + _lineCount + "Text", "[info] " + message);
                _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#0FF");
                _lineCount++;
                _runtimeServer.Set("processLogLineCount", _lineCount);
            }
        }

	    public void WriteError(string message)
        {
            lock (_lineLock)
            {
                _runtimeServer.Set("processLogLine" + _lineCount + "Text", "[error] " + message);
                _runtimeServer.Set("processLogLine" + _lineCount + "Color", "#F00");
                _lineCount++;
                _runtimeServer.Set("processLogLineCount", _lineCount);
            }
        }
	}
}

