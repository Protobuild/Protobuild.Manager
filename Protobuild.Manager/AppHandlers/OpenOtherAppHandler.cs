using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class OpenOtherAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
            Console.WriteLine("Would prompt file open dialog here...");
        }
    }
}