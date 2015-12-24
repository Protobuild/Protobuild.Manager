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
                    TemplateName = "C# Console",
                    TemplateDescription = "A cross-platform console application that can work on Windows, Mac OS and Linux.",
                    TemplateURI = "http://protobuild.org/commons/Console",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
                    OptionVariants = new List<OptionalVariant>(),
                    AdditionalPlatforms = new List<string>(),
                },
                new TemplateInfo
                {
                    TemplateName = "C# Library",
                    TemplateDescription = "A cross-platform C# library that can work across desktop and mobile platforms.",
                    TemplateURI = "http://protobuild.org/commons/Console",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
                    OptionVariants = new List<OptionalVariant>(),
                    AdditionalPlatforms = new List<string>(),
                },
                new TemplateInfo
                {
                    TemplateName = "Protogame Empty Game",
                    TemplateDescription = "A cross-platform Protogame project that renders an blank screen.",
                    TemplateURI = "http://protobuild.org/Protogame/Template.Blank",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
                    OptionVariants = new List<OptionalVariant>(),
                    AdditionalPlatforms = new List<string>(),
                },
                new TemplateInfo
                {
                    TemplateName = "Protogame 2D Platformer",
                    TemplateDescription = "A cross-platform Protogame project that runs a 2D platformer.",
                    TemplateURI = "http://protobuild.org/Protogame/Template.Platformer",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
                    OptionVariants = new List<OptionalVariant>(),
                    AdditionalPlatforms = new List<string>(),
                },
                new TemplateInfo
                {
                    TemplateName = "MonoGame 2D Platformer",
                    TemplateDescription = "A cross-platform MonoGame project that runs a 2D platformer.",
                    TemplateURI = "http://protobuild.org/MonoGame/Template.2DPlatformer@develop",
                    AdditionalProtobuildVariants = new Dictionary<string, VariantOverlay>(),
                    AdditionalStandardProjectVariants = new Dictionary<string, VariantOverlay>(),
					OptionVariants = new List<OptionalVariant>(),
                    AdditionalPlatforms = new List<string>(),
                }
            };
        }
    }
}