namespace Protobuild.Manager
{
    public interface IIDEControl
    {
        void LoadSolution(string modulePath, string moduleName, string targetPlatform);
    }
}