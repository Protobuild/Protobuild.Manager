using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace Protobuild.Manager
{
    public class OnDiskTemplateSource : ITemplateSource
    {
        private readonly IBrandingEngine _brandingEngine;

        public OnDiskTemplateSource(IBrandingEngine brandingEngine)
        {
            _brandingEngine = brandingEngine;
        }

        private Dictionary<string, VariantOverlay> ProcessVariants(ArrayList list, string basePath)
        {
            var dict = new Dictionary<string, VariantOverlay>();
            foreach (var item in list.OfType<Dictionary<string, object>>())
            {
                var id = (string) item["ID"];
                var name = (string)item["Name"];
                var overlayRelative = (string)item["Overlay"];
                var overlay = overlayRelative != null ? Path.Combine(basePath, overlayRelative) : null;
                dict[id] = new VariantOverlay
                {
                    ID = id,
                    Name = name,
                    OverlayPath = overlay,
                    EnableServices = ((ArrayList)item["EnableServices"]).Cast<string>().ToArray(),
                    DisableServices = ((ArrayList)item["DisableServices"]).Cast<string>().ToArray(),
                };
            }
            return dict;
        }

        public List<TemplateInfo> GetTemplates()
        {
            var directory = new DirectoryInfo(_brandingEngine.TemplateSource.Substring(4).Replace('\\', Path.DirectorySeparatorChar));
            var templates = new List<TemplateInfo>();

            foreach (var info in directory.GetFiles("*.json"))
            {
                var serializer = new JavaScriptSerializer();

                using (var reader = new StreamReader(info.FullName))
                {
                    var str = reader.ReadToEnd();
                    var data = serializer.Deserialize<Dictionary<string, object>>(str);

                    var name = (string) data["Name"];
                    var description = (string) data["Description"];
                    var protobuildVariants = ProcessVariants((ArrayList)data["ProtobuildVariants"], directory.FullName);
                    var standardVariants = ProcessVariants((ArrayList)data["StandardVariants"], directory.FullName);
                    var additionalPlatforms = ((ArrayList) data["AdditionalPlatforms"]).OfType<string>().ToList();
                    var optionalVariants = new List<OptionalVariant>();
                    var defaults = ((Dictionary<string, object>) data["Defaults"]).ToDictionary(k => k.Key, v => (string) v.Value);

                    foreach (var variant in ((ArrayList) data["OptionalVariants"]).OfType<Dictionary<string, object>>())
                    {
                        var newVariant = new OptionalVariant();
                        newVariant.ID = (string) variant["ID"];
                        newVariant.Name = (string)variant["Name"];
                        newVariant.ProtobuildOptions = ProcessVariants((ArrayList)variant["ProtobuildOptions"], directory.FullName).Select(x => x.Value).ToList();
                        newVariant.StandardOptions = ProcessVariants((ArrayList)variant["StandardOptions"], directory.FullName).Select(x => x.Value).ToList();

                        optionalVariants.Add(newVariant);
                    }
                    
                    templates.Add(new TemplateInfo
                    {
                        TemplateName = name,
                        TemplateDescription = description.Trim(),
                        TemplateURI = "local-template://" + Path.Combine(info.DirectoryName, info.Name.Substring(0, info.Name.Length - info.Extension.Length)),
                        AdditionalProtobuildVariants = protobuildVariants,
                        AdditionalStandardProjectVariants = standardVariants,
                        OptionVariants = optionalVariants,
                        AdditionalPlatforms = additionalPlatforms,
                        Defaults = defaults,
                    });
                }
            }

            return templates;
        }
    }
}
