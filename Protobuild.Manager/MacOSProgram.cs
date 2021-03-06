﻿#if PLATFORM_MACOS
namespace Protobuild.Manager
{
    using System;
    using System.Diagnostics;
#if PLATFORM_MACOS_LEGACY
    using MonoMac.AppKit;
#else
    using AppKit;
#endif

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                Run(args);
            }
            else
            {
                //AppDomain.CurrentDomain.UnhandledException +=
                //    (sender, e) => CrashReporter.Record((Exception)e.ExceptionObject);

                try
                {
                    Run(args);
                }
                catch (Exception e)
                {
                    //CrashReporter.Record(e);

                    Environment.Exit(1);
                }
            }
        }

        public static void Run(string[] args)
		{
			var kernel = new LightweightKernel();
			kernel.BindCommon();
			kernel.BindAndKeepInstance<IUIManager, MacOSUIManager>();
			kernel.BindAndKeepInstance<IExecution, MacOSExecution>();
			kernel.BindAndKeepInstance<IIDEControl, XamarinStudioMacIDEControl>();
            kernel.BindAndKeepInstance<IIDEProjectTemplateSync, NullProjectTemplateSync>();
            kernel.BindAndKeepInstance<IIDEAddinInstall, NullIDEAddinInstall>();

			kernel.Get<IErrorLog>().Log("Started Protobuild Manager on Mac platform");

			var startup = kernel.Get<IStartup>();
			startup.Start();
        }
    }
}
#endif

