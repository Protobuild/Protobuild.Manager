using System.IO;
using System.Reflection;

namespace Protobuild.Manager
{
    internal class ExternalBrandingEngine : BaseBrandingEngine
    {
        protected override Stream GetXmlStream()
        {
            var assemblyDirPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            var brandingPath = Path.Combine(assemblyDirPath, "Branding.xml");
            return new FileStream(brandingPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}