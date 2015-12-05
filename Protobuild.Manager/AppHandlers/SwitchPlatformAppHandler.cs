using System;
using System.Collections.Specialized;

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
            
            _ideControl.LoadSolution(@"C:\Users\June\Documents\Projects\MonoGame", "MonoGame.Framework", parameters["target"]);
        }
    }
}