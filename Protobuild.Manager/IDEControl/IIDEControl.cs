using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface IIDEControl
    {
        Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild);
    }
}