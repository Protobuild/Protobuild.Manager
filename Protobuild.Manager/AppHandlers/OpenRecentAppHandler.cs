using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class OpenRecentAppHandler : IAppHandler
    {
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IWorkflowManager _workflowManager;

        public OpenRecentAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
        }

        public void Handle(NameValueCollection parameters)
        {
            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(parameters["path"]));
        }
    }
}