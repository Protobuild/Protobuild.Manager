namespace Unearth
{
    public interface IOfflineDetection
    {
        bool Offline { get; }

        void MarkAsOffline();
    }
}