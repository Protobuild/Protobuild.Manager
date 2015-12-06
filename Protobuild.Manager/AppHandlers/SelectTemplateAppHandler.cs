using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class SelectTemplateAppHandler : IAppHandler
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly RuntimeServer _runtimeServer;

        public SelectTemplateAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Set("templateurl", parameters["url"]);

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectNamingWorkflow());
        }
    }
}
