using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class CreateProjectRequest
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string ProjectFormat { get; set; }
        public TemplateInfo Template { get; set; }
        public NameValueCollection Parameters { get; set; }
    }
}