using System;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class RegisterAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
            Process.Start("http://discoverunearth.com/auth/register/");
        }
    }
}

