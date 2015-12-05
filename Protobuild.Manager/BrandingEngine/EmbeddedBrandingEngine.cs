using System.Drawing;
using System.Xml;
using System.Linq;
using System.IO;

namespace Protobuild.Manager
{
    internal class EmbeddedBrandingEngine : IBrandingEngine
    {
        private readonly XmlDocument _brandingXml;

        public EmbeddedBrandingEngine()
        {
            _brandingXml = new XmlDocument();
            _brandingXml.Load(
                System.Reflection.Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Branding.xml"));
        }

        private string GetXmlString(string name)
        {
            return _brandingXml.DocumentElement.SelectNodes(name).OfType<XmlElement>().First().InnerText.Trim();
        }

        private Stream GetBinaryStreamFromXmlReference(string name)
        {
            var @ref = GetXmlString(name);
            if (@ref.StartsWith("@"))
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(@ref.Substring(1));
            }
            else
            {
                return new FileStream(@ref, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        public string ProductName => GetXmlString("ProductName");

        public string ProductStorageID => GetXmlString("ProductStorageID");

        public string RSSFeedURL => GetXmlString("RSSFeedURL");

#if PLATFORM_WINDOWS
        public Icon WindowsIcon => new Icon(GetBinaryStreamFromXmlReference("WindowsIcon"));
#elif PLATFORM_LINUX
        public Gdk.Pixbuf LinuxIcon => new Gdk.Pixbuf(GetBinaryStreamFromXmlReference("LinuxIcon"));
#endif
    }
}