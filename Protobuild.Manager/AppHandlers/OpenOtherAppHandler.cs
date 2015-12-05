using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class OpenOtherAppHandler : IAppHandler
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;

        public OpenOtherAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
        }

        public void Handle(NameValueCollection parameters)
        {
            Console.WriteLine("Would prompt file open dialog here...");

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(@"C:\Users\June\Documents\Projects\MonoGame", "MonoGame.Framework"));
        }
    }
}