#if PLATFORM_LINUX
using System;
using Gtk;
using WebKit;
using GLib;
using System.Web;

namespace Protobuild.Manager
{
    internal class LinuxUIManager : IUIManager
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IAppHandlerManager m_AppHandlerManager;

        private readonly IBrandingEngine _brandingEngine;

        internal LinuxUIManager(RuntimeServer server, IAppHandlerManager appHandlerManager, IBrandingEngine brandingEngine)
        {
            this.m_RuntimeServer = server;
            this.m_AppHandlerManager = appHandlerManager;
            _brandingEngine = brandingEngine;
        }

        public void Run()
        {
            Application.Init();
            Window window = new Window(_brandingEngine.ProductName + " (Init...)");
            window.Icon = _brandingEngine.LinuxIcon;
            window.SetSizeRequest(720, 400);
            window.Destroyed += delegate (object sender, EventArgs e)
            {
                Application.Quit();
            };
            ScrolledWindow scrollWindow = new ScrolledWindow();
            WebView webView = new WebView();
            webView.SetSizeRequest(720, 400);
            webView.Transparent = true;
            webView.BorderWidth = 0;
            scrollWindow.Add(webView);
            window.Add(scrollWindow);
            window.WindowPosition = WindowPosition.Center;
            window.AllowGrow = false;
            window.AllowShrink = false;
            window.BorderWidth = 0;
            window.ShowAll();

            this.m_RuntimeServer.RegisterRuntimeInjector(x => 
                Application.Invoke((oo, aa) => webView.ExecuteScript(x)));

            webView.LoadCommitted += (o, a) => window.Title = _brandingEngine.ProductName;
            webView.LoadFinished += (o, a) => window.Title = _brandingEngine.ProductName;
            webView.LoadProgressChanged += (o, a) => window.Title = _brandingEngine.ProductName + " (" + a.Progress + "% Loaded)";
            webView.LoadStarted += (o, a) => window.Title = _brandingEngine.ProductName + " (0% Loaded)";

            try
            {
                var signal = Signal.Lookup(webView, "resource-request-starting", typeof(SignalArgs));
                signal.AddDelegate(new SignalDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Starting: " + resource.Uri);
                }));

                signal = Signal.Lookup(webView, "resource-load-finished", typeof(SignalArgs));
                signal.AddDelegate(new SignalDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Load finished: " + resource.Uri);
                }));

                /*signal = Signal.Lookup(webView, "resource-content-length-received", typeof(SignalArgs));
                signal.AddDelegate(new SignalDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    //Console.WriteLine("Content length received: " + resource.Uri);
                }));*/

                signal = Signal.Lookup(webView, "resource-response-received", typeof(SignalArgs));
                signal.AddDelegate(new SignalDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Resource response received: " + resource.Uri);
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            webView.LoadUri(this.m_RuntimeServer.BaseUri);

            webView.NavigationRequested += (o, a) => {
                var url = a.Request.Uri;
                var uri = new Uri(url);

                this.m_AppHandlerManager.Handle(uri.AbsolutePath, HttpUtility.ParseQueryString(uri.Query));

                webView.StopLoading();
            };

            Application.Run();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
#endif
