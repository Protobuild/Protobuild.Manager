using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class TemplateInfo
    {
        public string TemplateName { get; set; }

        public string TemplateDescription { get; set; }

        public string TemplateURI { get; set; }

        public Dictionary<string, BaseVariant> AdditionalStandardProjectVariants { get; set; }

        public Dictionary<string, BaseVariant> AdditionalProtobuildVariants { get; set; } 

        public List<OptionalVariant> OptionVariants { get; set; }
    }

    public class BaseVariant
    {
        public string Name { get; set; }

        public string OverlayPath { get; set; }
    }
}