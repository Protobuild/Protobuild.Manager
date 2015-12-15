using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Reflection;

namespace Protobuild.Manager
{
    public class MonoDevelopProjectTemplateSync : IIDEProjectTemplateSync
    {
        private readonly ITemplateSource _templateSource;
        private readonly IBrandingEngine _brandingEngine;

        public MonoDevelopProjectTemplateSync(ITemplateSource templateSource, IBrandingEngine brandingEngine)
        {
            _templateSource = templateSource;
            _brandingEngine = brandingEngine;
        }

        public void Sync()
        {
            Task.Run(async () => await SyncInternal());
        }

        private async Task SyncInternal()
        {
            var templateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".xamarin-templates");
            var trimmedStorageId = _brandingEngine.ProductStorageID.TrimStart('.');

            using (var writer = new StreamWriter(Path.Combine(templateDirectory, trimmedStorageId + ".category")))
            {
                writer.Write(_brandingEngine.ProductName);
            }

            var assemblyPath = Assembly.GetEntryAssembly().Location;
            var workingDirectory = Environment.CurrentDirectory;

            var serializer = new JavaScriptSerializer();

            foreach (var template in _templateSource.GetTemplates())
            {
                var dict = new Dictionary<string, object>();

                dict["TemplateId"] = template.TemplateName;
                dict["TemplateName"] = template.TemplateName;
                dict["TemplateCategory"] = trimmedStorageId;
                dict["TemplateDescription"] = template.TemplateDescription;
                dict["ProtobuildManagerExecutablePath"] = assemblyPath;
                dict["ProtobuildManagerWorkingDirectory"] = workingDirectory;
                dict["ProtobuildManagerTemplateURL"] = template.TemplateURI;

                using (var twriter = new StreamWriter(Path.Combine(templateDirectory, template.TemplateName + ".tpl")))
                {
                    twriter.Write(serializer.Serialize(dict));
                }
            }
        }
    }
}

