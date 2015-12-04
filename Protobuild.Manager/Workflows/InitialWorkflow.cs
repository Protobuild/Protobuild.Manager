using System;

namespace Unearth
{
    public class InitialWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IChannelLoader m_ChannelLoader;

        public InitialWorkflow(RuntimeServer runtimeServer, IChannelLoader channelLoader)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_ChannelLoader = channelLoader;
        }

        public void Run()
        {
            var username = string.Empty;
            var phid = string.Empty;
            var certificate = string.Empty;

            this.m_RuntimeServer.Set("view", "main");
            this.m_RuntimeServer.Set("availableChannelCount", 0);
            this.m_RuntimeServer.Set("newsCount", 0);
            this.m_RuntimeServer.Set("currentOptions", ConfigManager.LoadGameOptions());

            this.m_ChannelLoader.LoadInBackground();

            if (ConfigManager.GetSavedConfig(ref username, ref phid, ref certificate))
            {
                this.m_RuntimeServer.Set("username", username);
                this.m_RuntimeServer.Set("cachedPassword", true);
            }
            else
            {
                this.m_RuntimeServer.Set("cachedPassword", false);
            }

#if PLATFORM_WINDOWS
            this.m_RuntimeServer.Set("canFullCrashDump", true);
            this.m_RuntimeServer.Set("fullCrashDumpsEnabled", ConfigManager.IsFullCrashDumpsEnabled());
#else
            this.m_RuntimeServer.Set("canFullCrashDump", false);
#endif
        }
    }
}

