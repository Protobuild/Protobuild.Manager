using System;

namespace Unearth
{
    public interface IProgressRenderer
    {
        void Update(double x, TimeSpan ts);
    }
}

