using System.Collections.Specialized;

namespace Protobuild.Manager
{
	public interface IAppHandlerManager
	{
        void Handle(string absolutePath, NameValueCollection parameters);
	}
}

