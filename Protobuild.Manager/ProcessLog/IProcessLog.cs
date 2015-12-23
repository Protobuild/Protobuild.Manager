using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Protobuild.Manager
{
	public interface IProcessLog
	{
		void PrepareForAttachToProcess(Process process);

	    void AttachToProcess(Process process);

	    void WriteInfo(string message);

	    void WriteError(string message);
	}
}

