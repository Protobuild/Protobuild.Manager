using System;
using System.Collections.Specialized;

namespace Unearth
{
    public class OptionAppHandler : IAppHandler
    {
        private readonly RuntimeServer m_RuntimeServer;

        public OptionAppHandler(RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            ConfigManager.SaveGameOptions(parameters["state"]);

            this.m_RuntimeServer.Set("currentOptions", ConfigManager.LoadGameOptions());
        }
    }
}

