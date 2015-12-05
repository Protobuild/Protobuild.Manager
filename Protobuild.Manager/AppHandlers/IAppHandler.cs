using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public interface IAppHandler
    {
        void Handle(NameValueCollection parameters);
    }
}

