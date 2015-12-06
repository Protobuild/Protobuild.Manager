using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class CancelCreationAppHandler : IAppHandler
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly RuntimeServer _runtimeServer;

        public CancelCreationAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Goto("index");

            _workflowManager.AppendWorkflow(_workflowFactory.CreateInitialWorkflow());
        }
    }
}