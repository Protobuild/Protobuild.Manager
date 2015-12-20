using System.Threading.Tasks;

namespace Protobuild.Manager
{
    internal class NullProjectTemplateSync : IIDEProjectTemplateSync
    {
        public async Task Sync()
        {
            await Task.Yield();
        }
    }
}