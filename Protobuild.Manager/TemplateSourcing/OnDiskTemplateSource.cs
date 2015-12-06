using System.Collections.Generic;
using System.IO;

namespace Protobuild.Manager
{
    public class OnDiskTemplateSource : ITemplateSource
    {
        private readonly IBrandingEngine _brandingEngine;

        public OnDiskTemplateSource(IBrandingEngine brandingEngine)
        {
            _brandingEngine = brandingEngine;
        }

        public List<TemplateInfo> GetTemplates()
        {
            var directory = new DirectoryInfo(_brandingEngine.TemplateSource.Substring(4));
            var templates = new List<TemplateInfo>();

            foreach (var subdir in directory.GetDirectories())
            {
                var info = Path.Combine(subdir.FullName, "info.txt");
                if (!File.Exists(info))
                {
                    continue;
                }

                using (var reader = new StreamReader(info))
                {
                    var name = reader.ReadLine();
                    var description = reader.ReadToEnd();

                    templates.Add(new TemplateInfo
                    {
                        TemplateName = name,
                        TemplateDescription = description.Trim(),
                        TemplateURI = "local-template://" + subdir.FullName
                    });
                }
            }

            return templates;
        }
    }
}
