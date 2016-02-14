using System;

namespace Protobuild.Manager
{
    public interface IRecentProjectsManager : ILoadable
    {
        void AddEntry(string name, string path);
        void RemoveEntry(string path);
    }
}

