using System;
using System.Collections.Specialized;

namespace Unearth
{
    public class ClearCacheAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
            ConfigManager.ClearSavedConfig();
        }
    }
}

