using System;

namespace Protobuild.Manager
{
    public class InitialWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;
        private readonly IBrandingEngine _brandingEngine;

        public InitialWorkflow(RuntimeServer runtimeServer, IBrandingEngine brandingEngine)
        {
            this.m_RuntimeServer = runtimeServer;
            _brandingEngine = brandingEngine;
        }

        public void Run()
        {
            this.m_RuntimeServer.Set("productName", _brandingEngine.ProductName);
        }
    }
}

