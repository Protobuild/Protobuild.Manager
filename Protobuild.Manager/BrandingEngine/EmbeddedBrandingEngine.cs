using System.IO;

namespace Protobuild.Manager
{
    internal class EmbeddedBrandingEngine : BaseBrandingEngine
    {
        protected override Stream GetXmlStream()
        {
            return System.Reflection.Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Branding.xml");
        }
    }
}