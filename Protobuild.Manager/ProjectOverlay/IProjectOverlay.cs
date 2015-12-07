namespace Protobuild.Manager
{
    public interface IProjectOverlay
    {
        void ApplyProjectTemplateOverlay(string overlayFolder, string targetFolder, string name);
    }
}