using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;
using System.Threading;
using WebKit;
using System.Web;

namespace Protobuild.Manager
{
    public partial class MainWindowController : NSWindowController
    {
		private readonly IBrandingEngine _brandingEngine;

        #region Constructors

		public MainWindowController(IBrandingEngine brandingEngine) : base() {
			_brandingEngine = brandingEngine;
		}

        // Called when created from unmanaged code
        public MainWindowController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        // Called when created directly from a XIB file
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        // Call to load from the XIB/NIB file
        public MainWindowController() : base("MainWindow")
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
        }

        #endregion

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            this.Window.BackgroundColor = NSColor.Black;
			this.Window.Title = _brandingEngine.ProductName;

            this.WebViewOutlet.DrawsBackground = false;

			#if FALSE
            AuthLogic.StartSearchForChannels(
                snapshots =>
                {
                    this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("window.resetSnapshots();"));
                    foreach (var snapshot in snapshots)
                    {
                        var snapshot1 = snapshot;

                        this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("window.addSnapshot('" + snapshot1 + "');"));
                    }
                    this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("window.setActiveSnapshot('" + ConfigManager.LoadChannel() + "');"));
                });

            var monitor = true;

            var thread = new Thread(() =>
            {
                while (monitor)
                {
                    Thread.Sleep(100);
                    this.WebViewOutlet.InvokeOnMainThread(() =>
                    {
                        if ((this.WebViewOutlet.EstimatedProgress * 100).ToString("F0") != "0")
                        {
                            this.Window.Title = "Unearth (" + (this.WebViewOutlet.EstimatedProgress * 100).ToString("F0") + "% Loaded)";
                        }
                    });
                }
            });
            thread.IsBackground = true;
            thread.Start();

            this.WebViewOutlet.CommitedLoad += (o, a) => this.Window.Title = "Unearth";
            this.WebViewOutlet.FailedLoadWithError += (o, a) =>
            {
                if (monitor)
                {
                    monitor = false;

                    this.Window.Title = "Unearth (Failed to Load)";
                }
            };

            EventHandler<WebFrameErrorEventArgs> failedStartup = null;
            EventHandler<WebFrameEventArgs> startup = null;

            failedStartup = (o, a) =>
            {
                monitor = false;

                this.Window.Title = "Unearth";

                this.WebViewOutlet.FailedProvisionalLoad -= failedStartup;
                this.WebViewOutlet.FinishedLoad -= startup;

                Console.WriteLine("OFFLINE MODE DETECTED");

                this.WebViewOutlet.MainFrame.LoadHtmlString(
                    AuthLogic.GetOfflineHtml(),
                    new NSUrl("http://localhost/offline"));
            };

            startup = (o, a) =>
            {
                monitor = false;

                this.Window.Title = "Unearth";

                this.WebViewOutlet.FailedProvisionalLoad -= failedStartup;
                this.WebViewOutlet.FinishedLoad -= startup;

                this.WebViewOutlet.StringByEvaluatingJavaScriptFromString(AuthLogic.GetStartJavascript());

                AuthLogic.ReadyForChannels();
            };

            this.WebViewOutlet.FailedProvisionalLoad += failedStartup;
            this.WebViewOutlet.FinishedLoad += startup;

            this.Window.Title = "Unearth (0% Loaded)";
            this.WebViewOutlet.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(UrlConfig.BASE + "?platform=mac&version=" + VersionConfig.VersionNumber)));

            this.WebViewOutlet.DecidePolicyForNavigation += (o, a) => {
                var url = a.Request.Url.ToString();
                var uri = new Uri(url);

                switch (uri.AbsolutePath)
                {
                    case "/login":

                        var parameters = ParseQueryString(uri.Query);
                        AuthLogic.PerformAuth(
                            parameters["username"] ?? string.Empty,
                            parameters.ContainsKey("password") ? parameters["password"] : string.Empty,
                            parameters["cached"] == "true",
                            () => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("window.toWorking()"),
                            s => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("document.getElementById('welcome_status').innerHTML = '" + s + "';"));
                        },
                            s => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("document.getElementById('working_status').innerHTML = '" + s + "';"));
                        },
                            s => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString(
                                "document.getElementById('error_status').innerHTML = '" + s + "'; window.toNormal()"));
                        },
                            (x, ts) => {

                            var eta = "";
                            if (ts.TotalSeconds >= 1)
                            {
                                eta = "";
                                if (ts.TotalHours >= 1)
                                {
                                    eta += ts.TotalHours.ToString("F0") + " hrs ";
                                    eta += ((int)ts.Minutes).ToString("D2") + " mins ";
                                    eta += ((int)ts.Seconds).ToString("D2") + " secs ";
                                }
                                else if (ts.TotalMinutes >= 1)
                                {
                                    eta += ts.Minutes.ToString("F0") + " mins ";
                                    eta += ((int)ts.Seconds).ToString("D2") + " secs ";
                                }
                                else if (ts.TotalSeconds >= 1)
                                {
                                    eta += ts.Seconds.ToString("F0") + " secs ";
                                }
                                eta += "remaining";
                            }

                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("window.setUpdateProgress(" + x + ", '" + eta + "');"));
                        },
                            () => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.Window.Close());
                        });

                        break;
                    case "/register":

                        this.WebViewOutlet.StringByEvaluatingJavaScriptFromString(AuthLogic.PerformRegister());

                        break;
                    case "/offline":
                        WebView.DecideUse(a.DecisionToken);

                        return;
                    case "/offlineplay":

                        AuthLogic.PerformOfflinePlay(
                            s => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString("document.getElementById('status').innerHTML = '" + s + "';"));
                        },
                            s => {
                            this.WebViewOutlet.InvokeOnMainThread(() => this.WebViewOutlet.StringByEvaluatingJavaScriptFromString(@"
document.getElementById('status').innerHTML = '" + s + @"';
document.getElementById('status').style.color = 'red';
document.getElementById('status').style.fontStyle = 'bold';"));
                        },
                            () => {
                            this.Window.Close();
                        });

                        break;
                    case "/clearcache":

                        ConfigManager.ClearSavedConfig();

                        break;
                    case "/option":

                        var optparameters = ParseQueryString(uri.Query);
                        ConfigManager.SaveGameOptions(optparameters["state"]);

                        break;
                    case "/channel":

                        var chnlparameters = ParseQueryString(uri.Query);
                        ConfigManager.SaveChannel(chnlparameters["name"]);

                        break;
                }

                WebView.DecideIgnore(a.DecisionToken);
            };

			#endif
        }

        //strongly typed window accessor
        public new MainWindow Window
        {
            get
            {
                return (MainWindow)base.Window;
            }
        }

        /// <summary>
        /// Parses the query string.  Use this on Mac to avoid System.Web reference.
        /// </summary>
        public static Dictionary<string, string> ParseQueryString(string s)
        {
            var nvc = new Dictionary<string, string>();

            if(s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in System.Text.RegularExpressions.Regex.Split(s, "&"))
            {
                string[] singlePair = System.Text.RegularExpressions.Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], Uri.UnescapeDataString(singlePair[1]));
                }
                else
                {
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }
    }
}

