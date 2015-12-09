using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class AutomatedBuildAppHandler : IAppHandler
    {
        private readonly RuntimeServer _runtimeServer;

        public AutomatedBuildAppHandler(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            Task.Run(async () =>
            {
                await HandleInBackground();
            });
        }

        private async Task HandleInBackground()
        {
            var modulePath = _runtimeServer.Get<string>("loadedModulePath");
            var protobuild = Path.Combine(modulePath, "Protobuild.exe");
            try
            {
                _runtimeServer.Set("busy", true);
                _runtimeServer.Set("statusMode", "Processing");
                _runtimeServer.Set("status", "Performing automated build...");
                var process = Process.Start(new ProcessStartInfo(protobuild, "--automated-build")
                {
                    WorkingDirectory = modulePath,
                    UseShellExecute = false
                });
                if (process == null)
                {
                    throw new InvalidOperationException("can't spawn Protobuild");
                }
                await process.WaitForExitAsync();

                _runtimeServer.Set("statusMode", "Okay");
                _runtimeServer.Set("status", "Automated build completed successfully.");
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