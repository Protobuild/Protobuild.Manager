﻿#if PLATFORM_LINUX
using System;
using Gtk;
using WebKit;
using GLib;
using System.Web;
using System.IO;

namespace Protobuild.Manager
{
    internal class LinuxUIManager : IUIManager
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IAppHandlerManager m_AppHandlerManager;

        private readonly IBrandingEngine _brandingEngine;

        private Window _window;

        internal LinuxUIManager(RuntimeServer server, IAppHandlerManager appHandlerManager, IBrandingEngine brandingEngine)
        {
            this.m_RuntimeServer = server;
            this.m_AppHandlerManager = appHandlerManager;
            _brandingEngine = brandingEngine;
        }

        public void Run()
        {
            Application.Init();
            _window = new Window(_brandingEngine.ProductName + " (Init...)");
            _window.Icon = _brandingEngine.LinuxIcon;
            _window.SetSizeRequest(720, 400);
            _window.Destroyed += delegate (object sender, EventArgs e)
            {
                Application.Quit();
            };
            ScrolledWindow scrollWindow = new ScrolledWindow();
            WebView webView = new WebView();
            webView.SetSizeRequest(720, 400);
            webView.Transparent = true;
            webView.BorderWidth = 0;
            scrollWindow.Add(webView);
            _window.Add(scrollWindow);
            _window.WindowPosition = WindowPosition.Center;
            _window.AllowGrow = false;
            _window.AllowShrink = false;
            _window.BorderWidth = 0;
            _window.ShowAll();

            this.m_RuntimeServer.RegisterRuntimeInjector(x => 
                Application.Invoke((oo, aa) => webView.ExecuteScript(x)));

            webView.LoadCommitted += (o, a) => _window.Title = _brandingEngine.ProductName;
            webView.LoadFinished += (o, a) => _window.Title = _brandingEngine.ProductName;
            webView.LoadProgressChanged += (o, a) => _window.Title = _brandingEngine.ProductName + " (" + a.Progress + "% Loaded)";
            webView.LoadStarted += (o, a) => _window.Title = _brandingEngine.ProductName + " (0% Loaded)";

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

                if (uri.Scheme != "app")
                {
                    return;
                }

                this.m_AppHandlerManager.Handle(uri.AbsolutePath, HttpUtility.ParseQueryString(uri.Query));

                webView.StopLoading();
            };

            Application.Run();
        }

        public void Quit()
        {
            Application.Quit();
        }

        public string SelectExistingProject()
        {
            var title = "Select Protobuild Module";
            var ofd = new Gtk.FileChooserDialog(
                title,
                _window,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);
            while (true)
            {
                if (ofd.Run() == (int)ResponseType.Accept)
                {
                    var fileInfo = new FileInfo(Path.Combine(ofd.Filename, "Protobuild.exe"));
                    if (!fileInfo.Exists || fileInfo.Name.ToLowerInvariant() != "Protobuild.exe".ToLowerInvariant())
                    {
                        var md = new MessageDialog(ofd, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok,
                            "It doesn't look like that directory is a Protobuild module.  The directory " +
                            "should contain Protobuild.exe to be opened with this tool.");
                        md.Run();
                        md.Destroy();
                        continue;
                    }
                    else
                    {
                        var fname = ofd.Filename;
                        ofd.Destroy();
                        return fname;
                    }
                }
                else
                {
                    ofd.Destroy();
                    return null;
                }
            }
        }

        public bool AskToRepairCorruptProtobuild()
        {
            var dialog = 
                new MessageDialog(
                    _window,
                    DialogFlags.DestroyWithParent | DialogFlags.Modal,
                    MessageType.Question,
                    ButtonsType.YesNo,
                    "The version of Protobuild.exe in this module could not be loaded " +
                    "and may be corrupt.  Do you want to download the latest version " +
                    "of Protobuild to repair it (Yes), or fallback to the solutions that " +
                    "have already been generated (No)?",
                    "Unable to load Protobuild");
            var result = dialog.Run() == (int)ResponseType.Yes;
            dialog.Destroy();
            return result;
        }

        public void FailedToRepairCorruptProtobuild()
        {
            var dialog = new MessageDialog(
                _window,
                DialogFlags.DestroyWithParent | DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                "This program was unable to repair Protobuild and will now fallback " +
                "to using the existing solutions.",
                "Failed to repair Protobuild");
            dialog.Run();
            dialog.Destroy();
        }

        public void UnableToLoadModule()
        {
            var dialog = new MessageDialog(
                _window,
                DialogFlags.DestroyWithParent | DialogFlags.Modal,
                MessageType.Error,
                ButtonsType.Ok,
                "This program was unable to load the module or project definition " +
                "information.  Check that the Build/Module.xml and project " +
                "definition files are all valid XML and that they contain no " +
                "errors.  This program will now fallback to using the existing " +
                "solutions.");
            dialog.Run();
            dialog.Destroy();
        }

        public string BrowseForProjectDirectory()
        {
            var title = "Select Project Directory";
            var ofd = new Gtk.FileChooserDialog(
                title,
                _window,
                FileChooserAction.SelectFolder,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);
            while (true)
            {
                if (ofd.Run() == (int)ResponseType.Accept)
                {
                    if (new DirectoryInfo(ofd.Filename).GetFiles().Length > 0)
                    {
                        var md = new MessageDialog(ofd, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.YesNo,
                            "It doesn't look like the selected directory is empty.  You " +
                            "should ideally create new projects in empty directories.  " +
                            "Use it anyway?");
                        var result = md.Run() == (int)ResponseType.Yes;
                        md.Destroy();

                        if (result)
                        {
                            var fname = ofd.Filename;
                            ofd.Destroy();
                            return fname;
                        }

                        continue;
                    }
                    else
                    {
                        var fname = ofd.Filename;
                        ofd.Destroy();
                        return fname;
                    }
                }
                else
                {
                    ofd.Destroy();
                    return null;
                }
            }
        }
    }
}
#endif
