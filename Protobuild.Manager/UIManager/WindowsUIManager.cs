#if PLATFORM_WINDOWS
namespace Unearth
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Windows.Forms;

    public class WindowsUIManager : IUIManager
    {
        private Form m_ActiveForm;
        
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IAppHandlerManager m_AppHandlerManager;

        public WindowsUIManager(RuntimeServer server, IAppHandlerManager appHandlerManager)
        {
            this.m_RuntimeServer = server;
            this.m_AppHandlerManager = appHandlerManager;
        }

        public void Run()
        {
            BrowserEmulation.EnableLatestIE();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form();
            this.m_ActiveForm = form;
            form.Text = "Unearth";
            //form.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("Unearth.GameIcon.ico"));
            form.Width = 740;
            form.Height = 460;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;

            var webBrowser = new WebBrowser();
            webBrowser.Dock = DockStyle.Fill;
            form.Controls.Add(webBrowser);

            this.m_RuntimeServer.RegisterRuntimeInjector(x =>
                this.SafeInvoke(form, () => ExecuteScript(webBrowser, x)));

            if (!Debugger.IsAttached)
            {
                webBrowser.ScriptErrorsSuppressed = true;
            }

            webBrowser.Navigate(this.m_RuntimeServer.BaseUri);

            webBrowser.Navigating += (o, a) => ExecuteAndCatch(
                () =>
                {
                    var uri = a.Url;

                    if (uri.Scheme != "app")
                    {
                        return;
                    }

                    this.m_AppHandlerManager.Handle(uri.AbsolutePath, HttpUtility.ParseQueryString(uri.Query));

                    a.Cancel = true;
                });

            form.Show();
            form.FormClosing += (o, a) =>
            {
                if (!Program.IsSubmitting)
                {
                    Application.Exit();
                }
            };

            Application.Run();
        }

        public void Quit()
        {
            Application.Exit();
        }

        private void ExecuteAndCatch(Action eventHandler)
        {
            if (Debugger.IsAttached)
            {
                eventHandler();
            }
            else
            {
                try
                {
                    eventHandler();
                }
                catch (Exception e)
                {
                    lock (Program.SubmissionLock)
                    {
                        if (Program.IsSubmitting)
                        {
                            return;
                        }

                        Program.IsSubmitting = true;
                    }

                    if (this.m_ActiveForm != null)
                    {
                        this.m_ActiveForm.Close();
                    }

                    Application.DoEvents();

                    Application.Exit();
                }
            }
        }

        private void SafeInvoke(Form form, Action action)
        {
            try
            {
                form.Invoke(action);
            }
            catch (InvalidOperationException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        [ComImport, ComVisible(true), Guid(@"3050f28b-98b5-11cf-bb82-00aa00bdce0b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
        [TypeLibType(TypeLibTypeFlags.FDispatchable)]
        public interface IHTMLScriptElement
        {
            [DispId(1006)]
            string text { set; [return: MarshalAs(UnmanagedType.BStr)] get; }
        }

        private static int m_NameCount = 0;

        private static void ExecuteScript(WebBrowser browser, string script)
        {
            var uniqueName = "call" + m_NameCount++;
            var head = browser.Document.GetElementsByTagName("head")[0];
            var scriptEl = browser.Document.CreateElement("script");
            var element = (IHTMLScriptElement)scriptEl.DomElement;
            element.text = "function " + uniqueName + "() { " + script + " }";
            head.AppendChild(scriptEl);
            browser.Document.InvokeScript(uniqueName);
        }
    }
}
#endif