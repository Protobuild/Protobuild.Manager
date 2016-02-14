using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class ResyncProjectsAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;
        private readonly RuntimeServer _runtimeServer;

        public ResyncProjectsAppHandler(IIDEControl ideControl, RuntimeServer runtimeServer)
        {
            _ideControl = ideControl;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            Task.Run(async () =>
            {
                await HandleInBackground(parameters["platform"], parameters["platform"], true);
            });
        }

        private async Task HandleInBackground(string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
        {
            await
                _ideControl.SaveAndSyncSolution(_runtimeServer.Get<string>("loadedModulePath"),
                    _runtimeServer.Get<string>("loadedModuleName"), targetPlatform, oldPlatformOnFail,
                isProtobuild);
            await
                _ideControl.CloseGenerateAndLoadSolution(_runtimeServer.Get<string>("loadedModulePath"),
                    _runtimeServer.Get<string>("loadedModuleName"), targetPlatform, oldPlatformOnFail,
                    isProtobuild);
        }
    }
}