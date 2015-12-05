using System;

namespace Protobuild.Manager
{
    public class InitialWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;

        public InitialWorkflow(RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
        }

        public void Run()
        {
            var username = string.Empty;
            var phid = string.Empty;
            var certificate = string.Empty;

            this.m_RuntimeServer.Set("view", "main");
            this.m_RuntimeServer.Set("availableChannelCount", 0);
            this.m_RuntimeServer.Set("newsCount", 0);
            /*this.m_RuntimeServer.Set("currentOptions", ConfigManager.LoadGameOptions());

            if (ConfigManager.GetSavedConfig(ref username, ref phid, ref certificate))
            {
                this.m_RuntimeServer.Set("username", username);
                this.m_RuntimeServer.Set("cachedPassword", true);
            }
            else
            {
                this.m_RuntimeServer.Set("cachedPassword", false);
            }*/

#if PLATFORM_WINDOWS
            this.m_RuntimeServer.Set("canFullCrashDump", true);
            this.m_RuntimeServer.Set("fullCrashDumpsEnabled", ConfigManager.IsFullCrashDumpsEnabled());
#else
            this.m_RuntimeServer.Set("canFullCrashDump", false);
#endif
        }
    }
}

