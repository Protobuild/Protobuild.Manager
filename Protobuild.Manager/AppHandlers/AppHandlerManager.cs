using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Unearth
{
    public class AppHandlerManager : IAppHandlerManager
    {
        private Dictionary<string, IAppHandler> m_AppHandlers;

        private readonly LightweightKernel m_Kernel;

        private bool m_AppHandlersInit;

        public AppHandlerManager(LightweightKernel kernel)
        {
            this.m_Kernel = kernel;
            this.m_AppHandlers = new Dictionary<string, IAppHandler>();
            this.m_AppHandlersInit = false;
        }

        public void Handle(string absolutePath, NameValueCollection parameters)
        {
            if (!this.m_AppHandlersInit)
            {
                this.m_AppHandlers.Add("/login", this.m_Kernel.Get<LoginAppHandler>());
                this.m_AppHandlers.Add("/register", this.m_Kernel.Get<RegisterAppHandler>());
                this.m_AppHandlers.Add("/clearcache", this.m_Kernel.Get<ClearCacheAppHandler>());
                this.m_AppHandlers.Add("/channel", this.m_Kernel.Get<ChannelAppHandler>());
                this.m_AppHandlers.Add("/option", this.m_Kernel.Get<OptionAppHandler>());
                this.m_AppHandlers.Add("/enablefullcrashdumps", this.m_Kernel.Get<EnableFullCrashDumpsAppHandler>());
                this.m_AppHandlersInit = true;
            }

            if (this.m_AppHandlers.ContainsKey(absolutePath))
            {
                this.m_AppHandlers[absolutePath].Handle(parameters);
            }
        }
    }
}

