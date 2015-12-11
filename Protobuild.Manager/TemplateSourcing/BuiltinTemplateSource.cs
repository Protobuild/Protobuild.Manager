using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class BuiltinTemplateSource : ITemplateSource
    {
        public List<TemplateInfo> GetTemplates()
        {
            return new List<TemplateInfo>
            {
                new TemplateInfo
                {
                    TemplateName = "MonoGame 2D Platformer",
                    TemplateDescription = "A cross-platform MonoGame project that runs a 2D platformer.",
                    TemplateURI = "http://protobuild.org/MonoGame/Template.2DPlatformer@develop",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
					OptionVariants = new List<OptionalVariant>(),
                }
            };
        }
    }
}