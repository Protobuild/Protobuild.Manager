using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Protobuild.Manager
{
	public interface IProcessLog
	{
		void AttachToProcess(Process process);
	}
}

