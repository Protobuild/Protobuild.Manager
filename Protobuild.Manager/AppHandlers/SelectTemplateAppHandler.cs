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

        internal SelectTemplateAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, RuntimeServer runtimeServer, ITemplateSource templateSource)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _runtimeServer = runtimeServer;
            _templateSource = templateSource;
        }

        public void Handle(NameValueCollection parameters)
        {
            _runtimeServer.Set("templateurl", parameters["url"]);
            
            var templates = _templateSource.GetTemplates();
            
            foreach (var template in templates)
            {
                if (template.TemplateURI == parameters["url"])
                {
                    var a = 0;
                    foreach (var kv in template.AdditionalStandardProjectVariants)
                    {
                        _runtimeServer.Set("templateStandardVariantsID" + a, kv.Key);
                        _runtimeServer.Set("templateStandardVariantsName" + a, kv.Value);
                        a++;
                    }
                    _runtimeServer.Set("templateStandardVariantsCount", a);

                    a = 0;
                    foreach (var kv in template.AdditionalProtobuildVariants)
                    {
                        _runtimeServer.Set("templateProtobuildVariantsID" + a, kv.Key);
                        _runtimeServer.Set("templateProtobuildVariantsName" + a, kv.Value);
                        a++;
                    }
                    _runtimeServer.Set("templateProtobuildVariantsCount", a);

                    a = 0;
                    foreach (var kv in template.OptionVariants)
                    {
                        _runtimeServer.Set("templateOptionalVariantsID" + a, kv.ID);
                        _runtimeServer.Set("templateOptionalVariantsName" + a, kv.Name);
                        a++;
                    }
                    _runtimeServer.Set("templateOptionalVariantsCount", a);

                    break;
                }
            }

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectNamingWorkflow());
        }
    }
}
