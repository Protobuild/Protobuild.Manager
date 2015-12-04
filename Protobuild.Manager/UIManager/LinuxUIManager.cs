#if PLATFORM_LINUX
using System;
using Gtk;
using WebKit;
using GLib;
using System.Web;

namespace Unearth
{
    public class LinuxUIManager : IUIManager
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IAppHandlerManager m_AppHandlerManager;

        public LinuxUIManager(RuntimeServer server, IAppHandlerManager appHandlerManager)
        {
            this.m_RuntimeServer = server;
            this.m_AppHandlerManager = appHandlerManager;
        }

        public void Run()
        {
            Application.Init();
            Window window = new Window("Unearth (Init...)");
            window.Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), "Unearth.GameIcon.ico");
            window.SetSizeRequest(725, 425);
            window.Destroyed += delegate (object sender, EventArgs e)
            {
                Application.Quit();
            };
            ScrolledWindow scrollWindow = new ScrolledWindow();
            WebView webView = new WebView();
            webView.SetSizeRequest(725, 425);
            webView.Transparent = true;
            scrollWindow.Add(webView);
            window.Add(scrollWindow);
            window.WindowPosition = WindowPosition.Center;
            window.AllowGrow = false;
            window.AllowShrink = false;
            window.ShowAll();

            this.m_RuntimeServer.RegisterRuntimeInjector(x => 
                Application.Invoke((oo, aa) => webView.ExecuteScript(x)));

            webView.LoadCommitted += (o, a) => window.Title = "Unearth";
            webView.LoadFinished += (o, a) => window.Title = "Unearth";
            webView.LoadProgressChanged += (o, a) => window.Title = "Unearth (" + a.Progress + "% Loaded)";
            webView.LoadStarted += (o, a) => window.Title = "Unearth (0% Loaded)";

            try
            {
                var signal = Signal.Lookup(webView, "resource-request-starting", typeof(SignalArgs));
                signal.AddDelegate(new ResourceRequestDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Starting: " + resource.Uri);
                }));

                signal = Signal.Lookup(webView, "resource-load-finished", typeof(SignalArgs));
                signal.AddDelegate(new ResourceRequestDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Load finished: " + resource.Uri);
                }));

                signal = Signal.Lookup(webView, "resource-content-length-received", typeof(SignalArgs));
                signal.AddDelegate(new ResourceRequestDelegate((o, a) =>
                {
                    var resource = (WebResource)a.Args[1];
                    Console.WriteLine("Content length received: " + resource.Uri);
                }));

                signal = Signal.Lookup(webView, "resource-response-received", typeof(SignalArgs));
                signal.AddDelegate(new ResourceRequestDelegate((o, a) =>
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
