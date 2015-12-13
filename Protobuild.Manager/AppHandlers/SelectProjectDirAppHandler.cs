using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class SelectProjectDirAppHandler : IAppHandler
    {
        private readonly IUIManager _uiManager;

        private readonly RuntimeServer _runtimeServer;

        public SelectProjectDirAppHandler(IUIManager uiManager, RuntimeServer runtimeServer)
        {
            _uiManager = uiManager;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            var result = _uiManager.BrowseForProjectDirectory();
            if (result != null)
            {
                _runtimeServer.Set("selectedProjectDir", result);
                _runtimeServer.Flush();
                _runtimeServer.Set("selectedProjectDir", null);
            }
        }
    }
}

