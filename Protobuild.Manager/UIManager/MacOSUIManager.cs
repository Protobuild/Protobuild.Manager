#if PLATFORM_MACOS
using System;
using AppKit;

namespace Protobuild.Manager
{
	public class MacOSUIManager : IUIManager
	{
		public static LightweightKernel KernelReference;

		public MacOSUIManager(LightweightKernel kernel) {
			KernelReference = kernel;
		}

		public void Run ()
		{
			NSApplication.Init();
			NSApplication.Main(new string[0]);
		}

		public void Quit ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
