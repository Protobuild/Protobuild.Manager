using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class CloseProjectAppHandler : IAppHandler
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly RuntimeServer _runtimeServer;

        public CloseProjectAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Set("disableStateUpdate", true);

            _runtimeServer.Goto("index");

            _runtimeServer.Set("loadedModuleName", null);
            _runtimeServer.Set("loadedModulePath", null);

            _runtimeServer.Set("disableStateUpdate", false);

            _workflowManager.AppendWorkflow(_workflowFactory.CreateInitialWorkflow());
        }
    }
}
