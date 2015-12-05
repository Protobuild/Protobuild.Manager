using System;

namespace Protobuild.Manager
{
    public interface IErrorLog
    {
        void Log(Exception ex);

        void Log(string s);
    }
}

