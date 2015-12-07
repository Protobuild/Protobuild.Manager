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

        private Dictionary<string, BaseVariant> ProcessVariants(ArrayList list, string basePath)
        {
            var dict = new Dictionary<string, BaseVariant>();
            foreach (var item in list.OfType<Dictionary<string, object>>())
            {
                var id = (string) item["ID"];
                var name = (string)item["Name"];
                var overlay = Path.Combine(basePath, (string)item["Overlay"]);
                dict[id] = new BaseVariant
                {
                    Name = name,
                    OverlayPath = overlay
                };
            }
            return dict;
        }

        public List<TemplateInfo> GetTemplates()
        {
            var directory = new DirectoryInfo(_brandingEngine.TemplateSource.Substring(4));
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
                    var optionalVariants = new List<OptionalVariant>();

                    foreach (var variant in ((ArrayList) data["OptionalVariants"]).OfType<Dictionary<string, object>>())
                    {
                        var newVariant = new OptionalVariant();
                        newVariant.ID = (string) variant["ID"];
                        newVariant.Name = (string)variant["Name"];
                        newVariant.ProtobuildOptions = new List<OptionalVariantOverlay>();
                        newVariant.StandardOptions = new List<OptionalVariantOverlay>();

                        foreach (
                            var option in
                                ((ArrayList)variant["ProtobuildOptions"]).OfType<Dictionary<string, object>>())
                        {
                            newVariant.ProtobuildOptions.Add(new OptionalVariantOverlay
                            {
                                ID = (string)option["ID"],
                                Name = (string)option["Name"],
                                OverlayPath = Path.Combine(directory.FullName, (string)option["Overlay"]),
                            });
                        }

                        foreach (
                            var option in
                                ((ArrayList)variant["StandardOptions"]).OfType<Dictionary<string, object>>())
                        {
                            newVariant.StandardOptions.Add(new OptionalVariantOverlay
                            {
                                ID = (string)option["ID"],
                                Name = (string)option["Name"],
                                OverlayPath = Path.Combine(directory.FullName, (string)option["Overlay"]),
                            });
                        }

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
                    });
                }
            }

            return templates;
        }
    }
}
