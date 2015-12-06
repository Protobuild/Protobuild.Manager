namespace Protobuild.Manager
{
    public class ProjectNamingWorkflow : IWorkflow
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly RuntimeServer _runtimeServer;

        public ProjectNamingWorkflow(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
        }

        public void Run()
        {
            _runtimeServer.Goto("nameproject");
        }
    }
}