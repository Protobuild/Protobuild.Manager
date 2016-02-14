using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public abstract class LinuxIDEControl : IIDEControl
    {
        private readonly RuntimeServer _runtimeServer;

		private readonly IExecution _execution;

		private readonly IProcessLog _processLog;

		public LinuxIDEControl(RuntimeServer runtimeServer, IExecution execution, IProcessLog processLog)
        {
            _runtimeServer = runtimeServer;
			_execution = execution;
			_processLog = processLog;
        }

        public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            Process process;

			try
			{
	            if (isProtobuild)
	            {
	                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

					_runtimeServer.Set("busy", true);
					_runtimeServer.Set("statusMode", "Processing");
	                _runtimeServer.Set("status", "Synchronising for " + oldPlatformOnFail + " platform...");
					process = _execution.ExecuteConsoleExecutable(
						protobuild,
						"--sync " + oldPlatformOnFail,
						x =>
						{
							x.WorkingDirectory = modulePath;
							x.UseShellExecute = false;
							x.RedirectStandardOutput = true;
							x.RedirectStandardError = true;
						},
                        _processLog.PrepareForAttachToProcess);
					_processLog.AttachToProcess(process);
	                await process.WaitForExitAsync();
					if (process.ExitCode != 0)
					{
						throw new Exception("Non-zero exit code from Protobuild");
					}

					_runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
					process = _execution.ExecuteConsoleExecutable(
						protobuild,
						"--generate " + targetPlatform,
						x =>
						{
							x.WorkingDirectory = modulePath;
							x.UseShellExecute = false;
							x.RedirectStandardOutput = true;
							x.RedirectStandardError = true;
						},
                        _processLog.PrepareForAttachToProcess);
					_processLog.AttachToProcess(process);
					await process.WaitForExitAsync();
					if (process.ExitCode != 0)
					{
						throw new Exception("Non-zero exit code from Protobuild");
					}
            	}

				OpenIDE(modulePath, moduleName, targetPlatform);

                _runtimeServer.Set("statusMode", "Okay");
                _runtimeServer.Set("status", "Projects generated for " + targetPlatform + " successfully.");
			}
			catch (Exception ex)
			{
				_runtimeServer.Set("statusMode", "Error");
				_runtimeServer.Set("status", ex.ToString());
			}
			finally
			{
				_runtimeServer.Set("busy", false);
			}
        }

        protected abstract Task OpenIDE(string modulePath, string moduleName, string targetPlatform);

        public async Task SaveAndSyncSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail,
            bool isProtobuild)
        {
            if (isProtobuild)
            {
                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                try
                {
                    _runtimeServer.Set("busy", true);
                    _runtimeServer.Set("statusMode", "Processing");
					_runtimeServer.Set("status", "Synchronising for " + targetPlatform + " platform...");
					var process = _execution.ExecuteConsoleExecutable(
						protobuild,
						"--sync " + targetPlatform,
						x =>
						{
							x.WorkingDirectory = modulePath;
                            x.UseShellExecute = false;
                            x.RedirectStandardOutput = true;
                            x.RedirectStandardError = true;
                        });
                    _processLog.AttachToProcess(process);
                    await process.WaitForExitAsync();

                    _runtimeServer.Set("statusMode", "Okay");
                    _runtimeServer.Set("status", "Projects synchronised for " + targetPlatform + " successfully.");
                }
                catch (Exception exx)
                {
                    _runtimeServer.Set("statusMode", "Error");
                    _runtimeServer.Set("status", exx.ToString());
                }
                finally
                {
                    _runtimeServer.Set("busy", false);
                }
            }
        }

        public async Task CloseGenerateAndLoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail,
            bool isProtobuild)
        {
            if (isProtobuild)
            {
                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                try
                {
                    _runtimeServer.Set("busy", true);
                    _runtimeServer.Set("statusMode", "Processing");
					_runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
					var process = _execution.ExecuteConsoleExecutable(
						protobuild,
						"--generate " + targetPlatform,
						x =>
						{
							x.WorkingDirectory = modulePath;
                            x.UseShellExecute = false;
                            x.RedirectStandardOutput = true;
                            x.RedirectStandardError = true;
                        });
                    _processLog.AttachToProcess(process);
                    await process.WaitForExitAsync();

                    _runtimeServer.Set("statusMode", "Okay");
                    _runtimeServer.Set("status", "Projects generated for " + targetPlatform + " successfully.");
                }
                catch (Exception exx)
                {
                    _runtimeServer.Set("statusMode", "Error");
                    _runtimeServer.Set("status", exx.ToString());
                }
                finally
                {
                    _runtimeServer.Set("busy", false);
                }
            }
        }
    }
}
