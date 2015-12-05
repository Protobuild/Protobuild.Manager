using System;
using System.Diagnostics;

namespace Protobuild.Manager
{
    public interface IExecution
    {
        void ExecuteConsoleExecutable(string path);

        void ExecuteApplicationExecutable(string path);
    }
}

