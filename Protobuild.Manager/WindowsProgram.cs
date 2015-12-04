#if PLATFORM_WINDOWS
namespace Unearth
{
    using System;
    using System.Diagnostics;
    using CrashReport;

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Run(args);
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, e) => CrashReporter.Record((Exception)e.ExceptionObject);

                try
                {
                    Run(args);
                }
                catch (Exception e)
                {
                    lock (SubmissionLock)
                    {
                        if (IsSubmitting)
                        {
                            return;
                        }

                        IsSubmitting = true;
                    }

                    CrashReporter.Record(e);
                }
            }
        }

        public static bool IsSubmitting;

        public static object SubmissionLock = new object();

        public static void Run(string[] args)
        {
            ErrorLog.Log("Started game launcher on Windows platform");

            var kernel = new LightweightKernel();
            kernel.BindCommon();
            kernel.BindAndKeepInstance<IUIManager, WindowsUIManager>();
            kernel.BindAndKeepInstance<IExecution, WindowsExecution>();

            kernel.Bind<IPrerequisiteCheck, DirectXPrerequisiteCheck>();

            var startup = kernel.Get<IStartup>();
            startup.Start();
        }
    }
}
#endif

