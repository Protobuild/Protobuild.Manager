using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Protobuild.Manager
{
    internal class VisualStudioIDEControl : IIDEControl
    {
        public void LoadSolution(string modulePath, string moduleName, string targetPlatform)
        {
            var existing = FindExistingVisualStudioInstance(modulePath, moduleName);
            if (existing != null)
            {
                existing = existing.DTE;
                //var resolvedType = Microsoft.VisualBasic.Information.TypeName(existing);


                existing.ExecuteCommand("File.SaveAll");

                // TODO: Perform sync + generate

                existing.Solution.Open(Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));
                existing.ActiveWindow.Activate();
                return;
            }

            Process.Start(
                @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe",
                Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln"));

            //dynamic dte = System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

            // Search for existing windows that have the solution open.
            // var window = dte.Windows;

        }

        private dynamic FindExistingVisualStudioInstance(string modulePath, string moduleName)
        {
            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            int retVal = GetRunningObjectTable(0, out rot);

            if (retVal == 0)
            {
                rot.EnumRunning(out enumMoniker);

                IntPtr fetched = IntPtr.Zero;
                IMoniker[] moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);
                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);

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
            }

            return null;
        }

        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);
    }
}
