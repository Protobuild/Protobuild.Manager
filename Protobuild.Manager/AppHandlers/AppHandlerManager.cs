using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class AppHandlerManager : IAppHandlerManager
    {
        private Dictionary<string, IAppHandler> _appHandlers;

        private readonly LightweightKernel _kernel;

        private bool _appHandlersInit;

        public AppHandlerManager(LightweightKernel kernel)
        {
            this._kernel = kernel;
            this._appHandlers = new Dictionary<string, IAppHandler>();
            this._appHandlersInit = false;
        }

        public void Handle(string absolutePath, NameValueCollection parameters)
        {
            if (!this._appHandlersInit)
            {
                _appHandlers.Add("/open-other", _kernel.Get<OpenOtherAppHandler>());
                _appHandlers.Add("/open-recent", _kernel.Get<OpenRecentAppHandler>());
                _appHandlers.Add("/create-new", _kernel.Get<CreateNewAppHandler>());
                _appHandlers.Add("/switch-platform", _kernel.Get<SwitchPlatformAppHandler>());
                _appHandlers.Add("/close", _kernel.Get<CloseProjectAppHandler>());
                _appHandlers.Add("/select-template", _kernel.Get<SelectTemplateAppHandler>());
                _appHandlers.Add("/cancel-creation", _kernel.Get<CancelCreationAppHandler>());
                _appHandlers.Add("/finalize-project", _kernel.Get<FinalizeProjectAppHandler>());
                _appHandlers.Add("/sync-projects", _kernel.Get<SyncProjectsAppHandler>());
                _appHandlers.Add("/generate-projects", _kernel.Get<GenerateProjectsAppHandler>());
                _appHandlers.Add("/create-package", _kernel.Get<CreatePackageAppHandler>());
                _appHandlers.Add("/automated-build", _kernel.Get<AutomatedBuildAppHandler>());

                //this.m_AppHandlers.Add("/login", this.m_Kernel.Get<LoginAppHandler>());
                //this.m_AppHandlers.Add("/register", this.m_Kernel.Get<RegisterAppHandler>());
                //this.m_AppHandlers.Add("/clearcache", this.m_Kernel.Get<ClearCacheAppHandler>());
                //this.m_AppHandlers.Add("/channel", this.m_Kernel.Get<ChannelAppHandler>());
                //this.m_AppHandlers.Add("/option", this.m_Kernel.Get<OptionAppHandler>());
                //this.m_AppHandlers.Add("/enablefullcrashdumps", this.m_Kernel.Get<EnableFullCrashDumpsAppHandler>());
                this._appHandlersInit = true;
            }

            if (this._appHandlers.ContainsKey(absolutePath))
            {
                this._appHandlers[absolutePath].Handle(parameters);
            }
        }
    }
}

