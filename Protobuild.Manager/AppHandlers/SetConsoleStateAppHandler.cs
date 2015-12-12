using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class SetConsoleStateAppHandler : IAppHandler
    {
        private readonly RuntimeServer _runtimeServer;

        public SetConsoleStateAppHandler(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Set("rememberedState", parameters["state"], true);
        }
    }
}