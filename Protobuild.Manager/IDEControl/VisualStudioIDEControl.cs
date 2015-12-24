using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    internal class VisualStudioIDEControl : IIDEControl
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IProcessLog _processLog;
        private readonly IExecution _execution;

        public VisualStudioIDEControl(RuntimeServer runtimeServer, IProcessLog processLog, IExecution execution)
        {
            _runtimeServer = runtimeServer;
            _processLog = processLog;
            _execution = execution;
        }

        public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            try
            {
                _runtimeServer.Set("busy", true);
                _runtimeServer.Set("statusMode", "Processing");
                _runtimeServer.Set("status", "Searching for open Visual Studio instances...");

                var vspid = _runtimeServer.Get<int?>("visualstudiopid");

                Func<dynamic, Task> launchLogic = null;

                var existing = FindExistingVisualStudioInstance(modulePath, moduleName, vspid);
                if (existing == null)
                {
                    launchLogic = async dteRef =>
                    {
                        _runtimeServer.Set("status", "Starting Visual Studio...");
                        Process.Start(
                            @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe",
                            Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));

                        while (existing == null)
                        {
                            _runtimeServer.Set("status", "Waiting for Visual Studio to open...");

                            await Task.Delay(1000);

                            existing = FindExistingVisualStudioInstance(modulePath, moduleName, vspid);
                        }
                    };
                }
                else
                {
                    launchLogic = async dteRef =>
                    {
                        _runtimeServer.Set("status", "Opening solution for " + targetPlatform + "...");

                        dteRef.Solution.Open(Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));
                        dteRef.ActiveWindow.Activate();

                        await Task.Yield();
                    };
                }

                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                dynamic dte = null;
                if (existing != null)
                {
                    try
                    {
                        dte = existing.DTE;
                    }
                    catch (COMException ex)
                    {
                        unchecked
                        {
                            if (ex.HResult == (int) 0x8001010A)
                            {
                                _runtimeServer.Set("statusMode", "Error");
                                _runtimeServer.Set("status",
                                    "Visual Studio is currently busy; can't switch platforms right now!");
                                _runtimeServer.Set("setplatform", oldPlatformOnFail);
                                _runtimeServer.Set("setplatform", null);
                                return;
                            }
                        }

                        throw;
                    }

                    if (dte.Solution.IsOpen)
                    {
                        var oldName = new FileInfo(dte.Solution.FullName).Name;
                        oldName = oldName.Substring(0, oldName.LastIndexOf('.'));
                        var oldPlatform = oldName.Substring(oldName.LastIndexOf('.') + 1);

                        _runtimeServer.Set("status", "Found Visual Studio, saving all files before solution close...");
                        dte.ExecuteCommand("File.SaveAll");
                        dte.Solution.Close(true);

                        if (isProtobuild)
                        {
                            _runtimeServer.Set("status", "Synchronising for " + oldPlatform + " platform...");
                            var syncProcess = _execution.ExecuteConsoleExecutable(protobuild, "--sync " + oldPlatform, x =>
                            {
                                x.WorkingDirectory = modulePath;
                                x.UseShellExecute = false;
                                x.CreateNoWindow = true;
                                x.RedirectStandardOutput = true;
                                x.RedirectStandardError = true;
                            },
                            _processLog.PrepareForAttachToProcess);
                            _processLog.AttachToProcess(syncProcess);
                            await syncProcess.WaitForExitAsync();
                        }
                    }
                }

                if (isProtobuild)
                {
                    _runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
                    var process = _execution.ExecuteConsoleExecutable(protobuild, "--generate " + targetPlatform, x =>
                    {
                        x.WorkingDirectory = modulePath;
                        x.UseShellExecute = false;
                        x.CreateNoWindow = true;
                        x.RedirectStandardOutput = true;
                        x.RedirectStandardError = true;
                    },
                    _processLog.PrepareForAttachToProcess);
                    _processLog.AttachToProcess(process);
                    process.WaitForExit();
                }

                await launchLogic(dte);

                _runtimeServer.Set("statusMode", "Okay");
                _runtimeServer.Set("status", "Platforms switched successfully.");
            }
            catch (Exception exx)
            {
                _runtimeServer.Set("statusMode", "Error");
                _runtimeServer.Set("status", exx.ToString());
                _runtimeServer.Set("setplatform", oldPlatformOnFail);
                _runtimeServer.Set("setplatform", null);
            }
            finally
            {
                _runtimeServer.Set("busy", false);
            }
        }

        public async Task SaveAndSyncSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail,
            bool isProtobuild)
        {
            try
            {
                _runtimeServer.Set("busy", true);
                _runtimeServer.Set("statusMode", "Processing");
                _runtimeServer.Set("status", "Searching for open Visual Studio instances...");

                var vspid = _runtimeServer.Get<int?>("visualstudiopid");

                var existing = FindExistingVisualStudioInstance(modulePath, moduleName, vspid);
                if (existing == null)
                {
                    return;
                }

                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                dynamic dte = null;
                try
                {
                    dte = existing.DTE;
                }
                catch (COMException ex)
                {
                    unchecked
                    {
                        if (ex.HResult == (int)0x8001010A)
                        {
                            _runtimeServer.Set("statusMode", "Error");
                            _runtimeServer.Set("status",
                                "Visual Studio is currently busy; can't save all projects right now!");
                            return;
                        }
                    }

                    throw;
                }

                if (dte.Solution.IsOpen)
                {
                    var oldName = new FileInfo(dte.Solution.FullName).Name;
                    oldName = oldName.Substring(0, oldName.LastIndexOf('.'));
                    var oldPlatform = oldName.Substring(oldName.LastIndexOf('.') + 1);

                    _runtimeServer.Set("status", "Found Visual Studio, saving all files before synchronisation...");
                    dte.ExecuteCommand("File.SaveAll");

                    if (isProtobuild)
                    {
                        _runtimeServer.Set("status", "Synchronising for " + oldPlatform + " platform...");
                        var syncProcess = _execution.ExecuteConsoleExecutable(protobuild, "--sync " + oldPlatform, x =>
                        {
                            x.WorkingDirectory = modulePath;
                            x.UseShellExecute = false;
                            x.CreateNoWindow = true;
                            x.RedirectStandardOutput = true;
                            x.RedirectStandardError = true;
                        },
                        _processLog.PrepareForAttachToProcess);
                        _processLog.AttachToProcess(syncProcess);
                        await syncProcess.WaitForExitAsync();
                    }
                }

                _runtimeServer.Set("statusMode", "Okay");
                _runtimeServer.Set("status", "Synchronised projects successfully.");
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

        public async Task CloseGenerateAndLoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail,
            bool isProtobuild)
        {
            try
            {
                _runtimeServer.Set("busy", true);
                _runtimeServer.Set("statusMode", "Processing");
                _runtimeServer.Set("status", "Searching for open Visual Studio instances...");

                var vspid = _runtimeServer.Get<int?>("visualstudiopid");

                Func<dynamic, Task> launchLogic = null;

                var existing = FindExistingVisualStudioInstance(modulePath, moduleName, vspid);
                if (existing == null)
                {
                    launchLogic = async dteRef =>
                    {
                        _runtimeServer.Set("status", "Starting Visual Studio...");

                        var versions = new[] { "14.0", "12.0", "11.0", "10.0" };
                        var started = false;
                        foreach (var version in versions)
                        {
                            var idePath = @"C:\Program Files (x86)\Microsoft Visual Studio " + version +
                                       @"\Common7\IDE\devenv.exe";
                            if (File.Exists(idePath))
                            {
                                var process = Process.Start(idePath);
                                if (process != null)
                                {
                                    vspid = process.Id;
                                    started = true;
                                }
                                break;
                            }
                        }

                        if (!started)
                        {
                            _runtimeServer.Set("status", "Unable to find installed version of Visual Studio to start!");
                        }
                        else
                        {
                            while (existing == null)
                            {
                                _runtimeServer.Set("status", "Waiting for Visual Studio to open...");

                                await Task.Delay(1000);

                                existing = FindExistingVisualStudioInstance(modulePath, moduleName, vspid);
                            }

                            var didLoad = false;
                            while (!didLoad)
                            {
                                dynamic dteAfterStart;
                                try
                                {
                                    dteAfterStart = existing.DTE;
                                }
                                catch (COMException ex)
                                {
                                    unchecked
                                    {
                                        if (ex.HResult == (int) 0x8001010A)
                                        {
                                            continue;
                                        }
                                    }

                                    throw;
                                }
                                
                                dteAfterStart.Solution.Open(Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));
                                dteAfterStart.ActiveWindow.Activate();
                                didLoad = true;
                            }
                        }
                    };
                }
                else
                {
                    launchLogic = async dteRef =>
                    {
                        _runtimeServer.Set("status", "Opening solution for " + targetPlatform + "...");

                        dteRef.Solution.Open(Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));
                        dteRef.ActiveWindow.Activate();

                        await Task.Yield();
                    };
                }

                var protobuild = Path.Combine(modulePath, "Protobuild.exe");

                dynamic dte = null;
                if (existing != null)
                {
                    try
                    {
                        dte = existing.DTE;
                    }
                    catch (COMException ex)
                    {
                        unchecked
                        {
                            if (ex.HResult == (int)0x8001010A)
                            {
                                _runtimeServer.Set("statusMode", "Error");
                                _runtimeServer.Set("status",
                                    "Visual Studio is currently busy; can't switch platforms right now!");
                                _runtimeServer.Set("setplatform", oldPlatformOnFail);
                                _runtimeServer.Set("setplatform", null);
                                return;
                            }
                        }

                        throw;
                    }

                    if (dte.Solution.IsOpen)
                    {
                        _runtimeServer.Set("status", "Found Visual Studio, saving all files before solution close...");
                        dte.ExecuteCommand("File.SaveAll");
                        dte.Solution.Close(true);
                    }
                }

                if (isProtobuild)
                {
                    _runtimeServer.Set("status", "Generating for " + targetPlatform + " platform...");
                    var process = _execution.ExecuteConsoleExecutable(protobuild, "--generate " + targetPlatform, x =>
                    {
                        x.WorkingDirectory = modulePath;
                        x.UseShellExecute = false;
                        x.CreateNoWindow = true;
                        x.RedirectStandardOutput = true;
                        x.RedirectStandardError = true;
                    },
                    _processLog.PrepareForAttachToProcess);
                    _processLog.AttachToProcess(process);
                    process.WaitForExit();
                }

                await launchLogic(dte);

                _runtimeServer.Set("statusMode", "Okay");
                _runtimeServer.Set("status", "Platforms switched successfully.");
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

        private dynamic FindExistingVisualStudioInstance(string modulePath, string moduleName, int? forcePid)
        {
            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            var retVal = GetRunningObjectTable(0, out rot);

            if (retVal == 0)
            {
                rot.EnumRunning(out enumMoniker);

                var fetched = IntPtr.Zero;
                var moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);

                    try
                    {
                        if (forcePid != null)
                        {
                            if ((displayName.StartsWith("!VisualStudio.DTE") || displayName.Contains("Express.DTE")) && displayName.EndsWith(":" + forcePid.Value))
                            {
                                // Always use this Visual Studio instance.
                                object obj;
                                rot.GetObject(moniker[0], out obj);
                                return obj;
                            }
                        }
                        else
                        {
                            if (displayName.StartsWith("!VisualStudio.DTE") || displayName.Contains("Express.DTE"))
                            {
                                // See if this Visual Studio instance has no solution open.
                                object obj;
                                rot.GetObject(moniker[0], out obj);
                                dynamic dte = obj;
                                if (!dte.Solution.IsOpen)
                                {
                                    // Re-use an empty instance.
                                    return dte;
                                }
                            }
                            
                            if (modulePath != null &&
                                displayName.ToLowerInvariant()
                                    .StartsWith(Path.Combine(modulePath, moduleName + ".").ToLowerInvariant()) &&
                                displayName.ToLowerInvariant().EndsWith(".sln"))
                            {
                                // Found an open solution for this module.
                                object obj;
                                rot.GetObject(moniker[0], out obj);
                                return obj;
                            }
                        }
                    }
                    catch (COMException ex)
                    {
                        unchecked
                        {
                            if (ex.HResult == (int) 0x8001010A)
                            {
                                continue;
                            }
                        }
                    }
                }
            }

            return null;
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}