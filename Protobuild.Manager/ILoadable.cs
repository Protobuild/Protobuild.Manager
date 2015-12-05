using System;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public interface ILoadable
    {
        Task Load();
    }
}

