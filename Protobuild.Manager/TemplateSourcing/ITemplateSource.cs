using System.Collections.Generic;

namespace Protobuild.Manager
{
    public interface ITemplateSource
    {
        List<TemplateInfo> GetTemplates();
    }
}