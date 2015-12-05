using System;
using System.Collections.Specialized;

namespace Protobuild.Manager
{
    public class CreateNewAppHandler : IAppHandler
    {
        public void Handle(NameValueCollection parameters)
        {
            Console.WriteLine("would move to create screen");
        }
    }
}