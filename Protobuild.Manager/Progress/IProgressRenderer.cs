using System;

namespace Protobuild.Manager
{
    public interface IProgressRenderer
    {
        void Update(double x, TimeSpan ts);
    }
}

