using System;
using System.Diagnostics;

namespace Unearth
{
    public interface IExecution
    {
        void ExecuteConsoleExecutable(string path);

        void ExecuteApplicationExecutable(string path);
    }
}

