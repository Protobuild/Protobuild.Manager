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

                        var b = 0;
                        foreach (var o in kv.ProtobuildOptions)
                        {
                            _runtimeServer.Set("templateOptionalVariantsProtobuildOption" + a + "ID" + b, o.ID);
                            _runtimeServer.Set("templateOptionalVariantsProtobuildOption" + a + "Name" + b, o.Name);
                            _runtimeServer.Set("templateOptionalVariantsProtobuildOption" + a + "OverlayPath" + b, o.OverlayPath);
                            b++;
                        }
                        _runtimeServer.Set("templateOptionalVariantsProtobuildOptionCount" + a, b);

                        b = 0;
                        foreach (var o in kv.StandardOptions)
                        {
                            _runtimeServer.Set("templateOptionalVariantsStandardOption" + a + "ID" + b, o.ID);
                            _runtimeServer.Set("templateOptionalVariantsStandardOption" + a + "Name" + b, o.Name);
                            _runtimeServer.Set("templateOptionalVariantsStandardOption" + a + "OverlayPath" + b, o.OverlayPath);
                            b++;
                        }
                        _runtimeServer.Set("templateOptionalVariantsStandardOptionCount" + a, b);

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
