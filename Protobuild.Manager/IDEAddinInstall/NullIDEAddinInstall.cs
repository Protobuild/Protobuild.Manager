using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class NullIDEAddinInstall : IIDEAddinInstall
    {
        public async Task InstallIfNeeded(bool force)
        {
            await Task.Yield();
        }
    }
}