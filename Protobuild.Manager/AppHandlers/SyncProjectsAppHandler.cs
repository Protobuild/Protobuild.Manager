using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class SyncProjectsAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;
        private readonly RuntimeServer _runtimeServer;

        public SyncProjectsAppHandler(IIDEControl ideControl, RuntimeServer runtimeServer)
        {
            _ideControl = ideControl;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            Task.Run(async () =>
            {
                await HandleInBackground(parameters["platform"], parameters["platform"], parameters["protobuild"] == "true");
            });
        }

        private async Task HandleInBackground(string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            await
                _ideControl.SaveAndSyncSolution(_runtimeServer.Get<string>("loadedModulePath"),
                    _runtimeServer.Get<string>("loadedModuleName"), targetPlatform, oldPlatformOnFail,
                    isProtobuild);
        }
    }
}