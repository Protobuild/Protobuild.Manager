using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface IIDEAddinInstall
    {
        Task InstallIfNeeded(bool force);
    }
}