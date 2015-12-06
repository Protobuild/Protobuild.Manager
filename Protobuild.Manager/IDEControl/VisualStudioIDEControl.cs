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

        public VisualStudioIDEControl(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform,
            string oldPlatformOnFail)
        {
            try
            {
                _runtimeServer.Set("status", "Searching for open Visual Studio instances...");

                Func<dynamic, Task> launchLogic = null;

                var existing = FindExistingVisualStudioInstance(modulePath, moduleName);
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

                            existing = FindExistingVisualStudioInstance(modulePath, moduleName);
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

                        _runtimeServer.Set("status", "Synchronising for " + oldPlatform + " platform...");
                        var syncProcess = Process.Start(new ProcessStartInfo(protobuild, "--sync " + oldPlatform)
                        {
                            WorkingDirectory = modulePath,
                            UseShellExecute = false
                        });
                        if (syncProcess == null)
                        {
                            throw new InvalidOperationException("can't sync");
                        }
                        syncProcess.WaitForExit();
                    }
                }

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
                process.WaitForExit();

                await launchLogic(dte);

                _runtimeServer.Set("status", "Platforms switched successfully.");
            }
            catch (Exception exx)
            {
                _runtimeServer.Set("status", exx.ToString());
                _runtimeServer.Set("setplatform", oldPlatformOnFail);
                _runtimeServer.Set("setplatform", null);
            }
        }

        private dynamic FindExistingVisualStudioInstance(string modulePath, string moduleName)
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
                        if (displayName.StartsWith("!VisualStudio.DTE"))
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

                        if (
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