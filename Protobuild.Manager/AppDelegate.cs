using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace Unearth
{
    [Register("AppDelegate")]
    public partial class AppDelegate : NSApplicationDelegate
    {
        private MainWindowController m_MainWindowController;

        public AppDelegate ()
        {
        }

        public override void FinishedLaunching(NSObject notification)
        {
            this.m_MainWindowController = new MainWindowController();
            this.m_MainWindowController.Window.MakeKeyAndOrderFront(this);
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }
}

