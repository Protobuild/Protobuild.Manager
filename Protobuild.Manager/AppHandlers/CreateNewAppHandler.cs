using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class CreateNewAppHandler : IAppHandler
    {
        private readonly ITemplateSource _templateSource;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IWorkflowManager _workflowManager;
        private readonly RuntimeServer _runtimeServer;

        internal CreateNewAppHandler(ITemplateSource templateSource, IWorkflowFactory workflowFactory, IWorkflowManager workflowManager, RuntimeServer runtimeServer)
        {
            _templateSource = templateSource;
            _workflowFactory = workflowFactory;
            _workflowManager = workflowManager;
            _runtimeServer = runtimeServer;
        }

        public void Handle(NameValueCollection parameters)
        {
            var templates = _templateSource.GetTemplates();

            var i = 0;
            foreach (var template in templates)
            {
                _runtimeServer.Set("templateName" + i, template.TemplateName);
                _runtimeServer.Set("templateDescription" + i, template.TemplateDescription);
                _runtimeServer.Set("templateURI" + i, template.TemplateURI);

                /*
                var a = 0;
                foreach (var kv in template.AdditionalStandardProjectVariants)
                {
                    _runtimeServer.Set("templateStandardVariants" + i + "_id" + a, kv.Key);
                    _runtimeServer.Set("templateStandardVariants" + i + "_name" + a, kv.Value);
                    a++;
                }
                _runtimeServer.Set("templateStandardVariantsCount", a);
                
                foreach (var kv in template.AdditionalStandardProjectVariants)
                {
                    _runtimeServer.Set("templateProtobuildVariants" + i + "_id" + a, kv.Key);
                    _runtimeServer.Set("templateProtobuildVariants" + i + "_name" + a, kv.Value);
                    a++;
                }
                _runtimeServer.Set("templateProtobuildVariantsCount", a);
                */

                i++;
            }

            _runtimeServer.Set("templateCount", i);

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectNewWorkflow());
        }
    }
}