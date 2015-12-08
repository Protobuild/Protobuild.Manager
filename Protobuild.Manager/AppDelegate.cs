using System;
using AppKit;
using Foundation;

namespace Protobuild.Manager
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {
        private MainWindowController m_MainWindowController;

        public AppDelegate ()
        {
        }

		public override void DidFinishLaunching (NSNotification notification)
		{
			this.m_MainWindowController = MacOSUIManager.KernelReference.Get<MainWindowController>();
			this.m_MainWindowController.Window.MakeKeyAndOrderFront(this);
		}

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }
}

