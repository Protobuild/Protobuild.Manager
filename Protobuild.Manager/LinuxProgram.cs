#if PLATFORM_LINUX
using System;
using System.Diagnostics;
using System.Web;
using GLib;
using Gtk;
using WebKit;
using System.IO;

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
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, e) => Console.Error.WriteLine((Exception)e.ExceptionObject);

                GLib.ExceptionManager.UnhandledException += (e) => 
                {
                    Console.Error.WriteLine((Exception)e.ExceptionObject);
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
                        // Try and extract webkit-sharp next to the assembly.
                        try
                        {
                            var assembly = typeof(Program).Assembly.GetManifestResourceStream("webkit-sharp.dll");
                            var assemblyConfig = typeof(Program).Assembly.GetManifestResourceStream("webkit-sharp.dll.config");

                            using (var writer = new FileStream(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "webkit-sharp.dll"), FileMode.Create, FileAccess.Write))
                            {
                                assembly.CopyTo(writer);
                            }
                            using (var writer = new FileStream(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "webkit-sharp.dll.config"), FileMode.Create, FileAccess.Write))
                            {
                                assemblyConfig.CopyTo(writer);
                            }

                            // We have to relaunch the executable to pick up the new libraries next to us.
                            var process = System.Diagnostics.Process.Start("mono", "\"" + typeof(Program).Assembly.Location + "\" " + string.Join(" ", args));
                            process.WaitForExit();

                            if (process.ExitCode == 1)
                            {
                                File.Delete(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "webkit-sharp.dll"));
                                File.Delete(Path.Combine(new FileInfo(typeof(Program).Assembly.Location).DirectoryName, "webkit-sharp.dll.config"));
                                ShowRequiredLibrary("webkit-sharp");
                            }

                            Environment.Exit(process.ExitCode);
                        }
                        catch
                        {
                            ShowRequiredLibrary("webkit-sharp");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine(e);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
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

