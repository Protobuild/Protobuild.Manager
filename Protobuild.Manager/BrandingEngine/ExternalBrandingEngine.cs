using System;
using System.IO;
using System.Reflection;

namespace Protobuild.Manager
{
    internal class ExternalBrandingEngine : BaseBrandingEngine
    {
        protected override Stream GetXmlStream()
        {
            var brandingPath = Path.Combine(Environment.CurrentDirectory, "Branding.xml");
            return new FileStream(brandingPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}