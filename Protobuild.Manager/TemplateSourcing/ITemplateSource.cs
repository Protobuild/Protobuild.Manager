using System.Collections.Generic;

namespace Protobuild.Manager
{
    internal interface ITemplateSource
    {
        List<TemplateInfo> GetTemplates();
    }
}