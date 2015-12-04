using System;
using System.Collections.Specialized;

namespace Unearth
{
    public class ChannelAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
            ConfigManager.SaveChannel(parameters["name"]);
        }
    }
}

