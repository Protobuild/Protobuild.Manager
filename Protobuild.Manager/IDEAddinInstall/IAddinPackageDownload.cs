using System;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface IAddinPackageDownload
    {
        Task<string> GetPackageRoot(string packageUrl);
    }
}

