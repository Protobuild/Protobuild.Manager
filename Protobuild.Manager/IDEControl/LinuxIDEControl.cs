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

        public LinuxIDEControl(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            Process process;

            if (isProtobuild)
            {
                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                _runtimeServer.Set("status", "Synchronising for " + oldPlatformOnFail + " platform...");
                process = Process.Start(new ProcessStartInfo(protobuild, "--sync " + oldPlatformOnFail)
                {
                    WorkingDirectory = modulePath,
                    UseShellExecute = false
                });
                if (process == null)
                {
                    throw new InvalidOperationException("can't sync");
                }
                await process.WaitForExitAsync();

                _runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
                process = Process.Start(new ProcessStartInfo(protobuild, "--generate " + targetPlatform)
                {
                    WorkingDirectory = modulePath,
                    UseShellExecute = false
                });
                if (process == null)
                {
                    throw new InvalidOperationException("can't generate");
                }
                await process.WaitForExitAsync();
            }

            await OpenIDE(modulePath, moduleName, targetPlatform);
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
                    _runtimeServer.Set("statusState", "Processing");
                    _runtimeServer.Set("status", "Synchronising for " + targetPlatform + " platform...");
                    var process = Process.Start(new ProcessStartInfo(protobuild, "--sync " + targetPlatform)
                    {
                        WorkingDirectory = modulePath,
                        UseShellExecute = false
                    });
                    if (process == null)
                    {
                        throw new InvalidOperationException("can't generate");
                    }
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
                    _runtimeServer.Set("statusState", "Processing");
                    _runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
                    var process = Process.Start(new ProcessStartInfo(protobuild, "--generate " + targetPlatform)
                    {
                        WorkingDirectory = modulePath,
                        UseShellExecute = false
                    });
                    if (process == null)
                    {
                        throw new InvalidOperationException("can't generate");
                    }
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
