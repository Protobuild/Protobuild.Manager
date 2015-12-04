#if PLATFORM_WINDOWS
namespace Unearth
{
    using System.IO;
    using System.Reflection;
    using Microsoft.Win32;

    public static class BrowserEmulation
    {
        public static void EnableLatestIE()
        {
            // Detect latest IE version
            var version = 9000;
            var verSrc = Registry.LocalMachine;
            foreach (var key in new[] { "Software", "Microsoft", "Internet Explorer" })
            {
                verSrc = verSrc.OpenSubKey(key);
            }
            var versionValue = verSrc.GetValue("Version");
            var svcVersionValue = verSrc.GetValue("svcVersion");
            var latestValue = (string)(svcVersionValue ?? versionValue);
            if (latestValue != null)
            {
                if (latestValue.StartsWith("8."))
                {
                    version = 8000;
                }
                else if (latestValue.StartsWith("9."))
                {
                    version = 9000;
                }
                else if (latestValue.StartsWith("10."))
                {
                    version = 10000;
                }
                else if (latestValue.StartsWith("11."))
                {
                    version = 11000;
                }
            }

            // Force latest IE mode for Web Browser control
            var keyNames = new[]
            { "Software", "Microsoft", "Internet Explorer", "Main", "FeatureControl", "FEATURE_BROWSER_EMULATION" };
            var src = Registry.CurrentUser;
            foreach (var key in keyNames)
            {
                src = src.CreateSubKey(key);
            }

            src.SetValue(new FileInfo(Assembly.GetExecutingAssembly().Location).Name, version);
            src.SetValue("Unearth.vshost.exe", version);
        }
    }
}
#endif