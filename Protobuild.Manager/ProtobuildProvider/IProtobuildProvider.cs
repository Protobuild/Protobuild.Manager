using System;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface IProtobuildProvider
    {
        Task<string> GetProtobuild(Action<string> updateStatus);
    }
}

