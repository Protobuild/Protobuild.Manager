using System.Collections.Specialized;

namespace Unearth
{
	public interface IAppHandlerManager
	{
        void Handle(string absolutePath, NameValueCollection parameters);
	}
}

