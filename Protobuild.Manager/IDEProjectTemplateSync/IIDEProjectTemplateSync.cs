using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface IIDEProjectTemplateSync
    {
        Task Sync();
    }
}