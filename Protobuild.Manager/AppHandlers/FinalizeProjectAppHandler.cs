using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Protobuild.Manager
{
    public class FinalizeProjectAppHandler : IAppHandler
    {
        private readonly ITemplateSource _templateSource;
        private readonly RuntimeServer _runtimeServer;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IWorkflowManager _workflowManager;
        private readonly IProjectCreator _projectCreator;

        public FinalizeProjectAppHandler(ITemplateSource templateSource, RuntimeServer runtimeServer, IWorkflowFactory workflowFactory, IWorkflowManager workflowManager, IProjectCreator projectCreator)
        {
            _templateSource = templateSource;
            _runtimeServer = runtimeServer;
            _workflowFactory = workflowFactory;
            _workflowManager = workflowManager;
            _projectCreator = projectCreator;
        }

        public void Handle(NameValueCollection parameters)
        {
            var name = parameters["name"];
            var path = parameters["path"];
            var projectFormat = parameters["projectFormat"];
            var template = _templateSource.GetTemplates().First(x => x.TemplateURI == _runtimeServer.Get<string>("templateurl"));

            var request = new CreateProjectRequest
            {
                Name = name,
                Path = path,
                ProjectFormat = projectFormat,
                Template = template,
                Parameters = parameters
            };

            _projectCreator.CreateProject(request);
        }
    }
}