using System.Drawing;

namespace Protobuild.Manager
{
    internal class DefaultBrandingEngine : IBrandingEngine
    {
        public string ProductName => "Protobuild";

        public string RSSFeedURL => "https://medium.com/feed/protobuild-development-updates";

#if PLATFORM_WINDOWS
        public Icon WindowsIcon => null;
#endif
    }
}