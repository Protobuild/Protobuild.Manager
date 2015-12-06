using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class SwitchPlatformAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;
        private readonly RuntimeServer _runtimeServer;

        public SwitchPlatformAppHandler(IIDEControl ideControl, RuntimeServer runtimeServer)
        {
            _ideControl = ideControl;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            Console.WriteLine("would move to create screen");

            Task.Run(async () =>
            {
                await HandleInBackground(parameters["target"], parameters["old"]);
            });
        }

        private async Task HandleInBackground(string targetPlatform, string oldPlatformOnFail)
        {
            await
                _ideControl.LoadSolution(_runtimeServer.Get<string>("loadedModulePath"),
                    _runtimeServer.Get<string>("loadedModuleName"), targetPlatform, oldPlatformOnFail);
        }
    }
}