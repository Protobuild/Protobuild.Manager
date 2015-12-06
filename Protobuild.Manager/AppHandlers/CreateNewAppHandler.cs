using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class CreateNewAppHandler : IAppHandler
    {
        private readonly IIDEControl _ideControl;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IWorkflowManager _workflowManager;

        public CreateNewAppHandler(IIDEControl ideControl, IWorkflowFactory workflowFactory, IWorkflowManager workflowManager)
        {
            _ideControl = ideControl;
            _workflowFactory = workflowFactory;
            _workflowManager = workflowManager;
        }

        public void Handle(NameValueCollection parameters)
        {
            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectNewWorkflow());
        }
    }
}