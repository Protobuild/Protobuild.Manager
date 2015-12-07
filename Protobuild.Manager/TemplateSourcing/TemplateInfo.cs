using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class TemplateInfo
    {
        public string TemplateName { get; set; }

        public string TemplateDescription { get; set; }

        public string TemplateURI { get; set; }

        public Dictionary<string, string> AdditionalStandardProjectVariants { get; set; }

        public Dictionary<string, string> AdditionalProtobuildVariants { get; set; } 

        public List<OptionalVariant> OptionVariants { get; set; }
    }
}