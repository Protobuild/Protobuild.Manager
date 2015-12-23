using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;

namespace Protobuild.Manager
{
    internal abstract class BaseBrandingEngine : IBrandingEngine
    {
        private readonly XmlDocument _brandingXml;

        public BaseBrandingEngine()
        {
            _brandingXml = new XmlDocument();
            _brandingXml.Load(GetXmlStream());
        }

        protected abstract Stream GetXmlStream();

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

        public string ProductName
        {
            get { return GetXmlString("ProductName"); }
        }

        public string ProductStorageID
        {
            get { return GetXmlString("ProductStorageID"); }
        }

        public string RSSFeedURL
        {
            get { return GetXmlString("RSSFeedURL"); }
        }

#if PLATFORM_WINDOWS
        public Icon WindowsIcon
        {
            get { return new Icon(GetBinaryStreamFromXmlReference("WindowsIcon")); }
        }
#elif PLATFORM_LINUX
        public Gdk.Pixbuf LinuxIcon
        {
            get { return new Gdk.Pixbuf(GetBinaryStreamFromXmlReference("LinuxIcon")); }
        }
#endif

        public string TemplateSource
        {
            get { return GetXmlString("TemplateSource"); }
        }

        public string TemplateIDECategory
        {
            get { return GetXmlString("TemplateIDECategory"); }
        }

        public string VisualStudioAddinPackage
        {
            get
            {
                var path = GetXmlString("VisualStudioAddinPackage");
                if (path.StartsWith("file:"))
                {
                    path = "local:///" + Path.GetFullPath(path.Substring(5));
                }
                return path;
            }
        }

        public string MonoDevelopAddinPackage
        {
            get
            {
                var path = GetXmlString("MonoDevelopAddinPackage");
                if (path.StartsWith("file:"))
                {
                    path = "local:///" + Path.GetFullPath(path.Substring(5));
                }
                return path;
            }
        }

        public ProtobuildUpdatePolicy ProtobuildUpdatePolicy
        {
            get
            {
                switch (GetXmlString("ProtobuildUpdatePolicy"))
                {
                    case "when-available":
                        return ProtobuildUpdatePolicy.WhenAvailable;
                    case "never":
                        return ProtobuildUpdatePolicy.Never;
                    default:
                        return ProtobuildUpdatePolicy.WhenAvailable;
                }
            }
        }
    }
}