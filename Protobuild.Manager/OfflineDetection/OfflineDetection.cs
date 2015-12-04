namespace Unearth
{
    public class OfflineDetection : IOfflineDetection
    {
        private readonly RuntimeServer m_RuntimeServer;

        public OfflineDetection(RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
            this.Offline = false;
        }

        public bool Offline { get; private set; }

        public void MarkAsOffline()
        {
            this.Offline = true;

            string username = null;
            string phid = null;
            string certificate = null;

            ConfigManager.GetSavedConfig(ref username, ref phid, ref certificate);

            if (username != null)
            {
                this.m_RuntimeServer.Set("username", username);
                this.m_RuntimeServer.Set("offlineplay", true);
            }

            this.m_RuntimeServer.Set("offline", true);
        }
    }
}