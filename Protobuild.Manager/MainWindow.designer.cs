// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
using MonoMac.WebKit;
#else
using Foundation;
using WebKit;
#endif
using System.CodeDom.Compiler;

namespace Protobuild.Manager
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		WebView WebViewOutlet { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (WebViewOutlet != null) {
				WebViewOutlet.Dispose ();
				WebViewOutlet = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
