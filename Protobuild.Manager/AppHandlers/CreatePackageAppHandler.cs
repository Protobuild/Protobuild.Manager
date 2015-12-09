using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class CreatePackageAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;
        private readonly RuntimeServer _runtimeServer;

        public CreatePackageAppHandler(IIDEControl ideControl, RuntimeServer runtimeServer)
        {
            _ideControl = ideControl;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            Task.Run(async () =>
            {
                await HandleInBackground(parameters["platform"]);
            });
        }

        private async Task HandleInBackground(string targetPlatform)
        {
            var modulePath = _runtimeServer.Get<string>("loadedModulePath");
            var protobuild = Path.Combine(modulePath, "Protobuild.exe");
            try
            {
                _runtimeServer.Set("busy", true);
                _runtimeServer.Set("statusMode", "Processing");
                _runtimeServer.Set("status", "Create package for " + targetPlatform + " platform...");
                var process = Process.Start(new ProcessStartInfo(protobuild, "--pack . " + targetPlatform + ".tar.lzma " + targetPlatform)
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
                _runtimeServer.Set("status", "Package created at: " + Path.Combine(modulePath, targetPlatform + ".tar.lzma"));
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