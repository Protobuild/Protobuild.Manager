using System;
using System.Collections.Specialized;

namespace Unearth
{
    public interface IAppHandler
    {
        void Handle(NameValueCollection parameters);
    }
}

