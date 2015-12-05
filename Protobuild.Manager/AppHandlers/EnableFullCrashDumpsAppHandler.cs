namespace Protobuild.Manager
{
    using System.Collections.Specialized;

    public class EnableFullCrashDumpsAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
#if PLATFORM_WINDOWS
            ConfigManager.EnableFullCrashDumps();
#endif
        }
    }
}