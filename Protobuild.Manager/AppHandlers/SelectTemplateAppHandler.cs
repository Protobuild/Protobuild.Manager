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
        private readonly ITemplateSource _templateSource;
        private readonly IProjectDefaultPath _projectDefaultPath;

        internal SelectTemplateAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer, ITemplateSource templateSource, IProjectDefaultPath projectDefaultPath)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
            _templateSource = templateSource;
            _projectDefaultPath = projectDefaultPath;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Set("templateurl", parameters["url"]);
            _runtimeServer.Set("defaultpath", _projectDefaultPath.GetProjectDefaultPath());

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectNamingWorkflow());
        }
    }
}
