namespace Protobuild.Manager
{
    public interface IProtobuildHostingEngine
    {
        ModuleHost LoadModule(string modulePath);
    }
}