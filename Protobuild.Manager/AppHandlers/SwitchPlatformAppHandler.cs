using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class SwitchPlatformAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;

        public SwitchPlatformAppHandler(IIDEControl ideControl)
        {
            _ideControl = ideControl;
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
            await _ideControl.LoadSolution(@"C:\Users\June\Documents\Projects\MonoGame", "MonoGame.Framework", targetPlatform, oldPlatformOnFail);
        }
    }
}