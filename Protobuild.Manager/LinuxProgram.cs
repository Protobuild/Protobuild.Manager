#if PLATFORM_LINUX
using System;
using System.Diagnostics;
using System.Web;
using GLib;
using Gtk;
using WebKit;

namespace Protobuild.Manager
{
    public delegate void SignalDelegate(object obj, SignalArgs args);

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                GLib.ExceptionManager.UnhandledException += (e) => 
                {
                    Console.Error.WriteLine(e.ExceptionObject);
                    Debugger.Break();
                };

                Run(args);
            }
            else
            {
                //AppDomain.CurrentDomain.UnhandledException +=
                //    (sender, e) => CrashReporter.Record((Exception)e.ExceptionObject);

                GLib.ExceptionManager.UnhandledException += (e) => 
                {
                    //CrashReporter.Record((Exception)e.ExceptionObject);
                    e.ExitApplication = true;
                };

                try
                {
                    Run(args);
                }
                catch (System.IO.FileNotFoundException e)
                {
                    if (e.Message.Contains("webkit-sharp"))
                    {
                        ShowRequiredLibrary("webkit-sharp");
                    }
                    else
                    {
                        //CrashReporter.Record(e);
                    }
                }
                catch (Exception e)
                {
                    //CrashReporter.Record(e);
                }
            }
        }

        public static void ShowRequiredLibrary(string library)
        {
            Application.Init();
            var window = new Window("Missing Library!");
            window.Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetExecutingAssembly(), "Unearth.GameIcon.ico");
            window.SetSizeRequest(300, 150);
            window.Destroyed += delegate (object sender, EventArgs e)
            {
                Application.Quit();
            };
            var label = new Label("Please install " + library + " using \nyour package manager!");
            window.Add(label);
            window.WindowPosition = WindowPosition.Center;
            window.AllowGrow = false;
            window.AllowShrink = false;
            window.ShowAll();
            Application.Run();
        }

        public static void Run(string[] args)
        {
            var kernel = new LightweightKernel();
            kernel.BindCommon();
            kernel.BindAndKeepInstance<IUIManager, LinuxUIManager>();
            kernel.BindAndKeepInstance<IExecution, LinuxExecution>();
            kernel.BindAndKeepInstance<IIDEControl, MonoDevelopLinuxIDEControl>();
            kernel.BindAndKeepInstance<IIDEProjectTemplateSync, MonoDevelopProjectTemplateSync>();

            kernel.Get<IErrorLog>().Log("Started game launcher on Linux platform");

            var startup = kernel.Get<IStartup>();
            startup.Start(args);
        }
    }
}
#endif

