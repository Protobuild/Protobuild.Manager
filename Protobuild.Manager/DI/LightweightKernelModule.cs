using System.Reflection;
using System.IO;

namespace Protobuild.Manager
{
    public static class LightweightKernelModule
    {
        public static void BindCommon(this LightweightKernel kernel)
        {
            var assemblyDirPath = new FileInfo(Assembly.GetEntryAssembly().Location).DirectoryName;
            var brandingPath = Path.Combine(assemblyDirPath, "Branding.xml");
            if (File.Exists(brandingPath))
            {
                // TODO: ExternalBindingEngine
                kernel.BindAndKeepInstance<IBrandingEngine, EmbeddedBrandingEngine>();
            }
            else
            {
                kernel.BindAndKeepInstance<IBrandingEngine, EmbeddedBrandingEngine>();
            }

            kernel.BindAndKeepInstance<IConfigManager, ConfigManager>();
            kernel.BindAndKeepInstance<IErrorLog, ErrorLog>();
            kernel.BindAndKeepInstance<RuntimeServer, RuntimeServer>();
            kernel.BindAndKeepInstance<IWorkflowManager, WorkflowManager>();
            kernel.BindAndKeepInstance<IAppHandlerManager, AppHandlerManager>();
            kernel.BindAndKeepInstance<IProgressRenderer, ProgressRenderer>();
            kernel.BindAndKeepInstance<IWorkflowFactory, WorkflowFactory>();
            //kernel.BindAndKeepInstance<IPathProvider, PathProvider>();
            //kernel.BindAndKeepInstance<INewsLoader, NewsLoader>();
            kernel.BindAndKeepInstance<ILauncherSelfUpdate, LauncherSelfUpdate>();
            kernel.BindAndKeepInstance<IStartup, Startup>();
            //kernel.BindAndKeepInstance<IOfflineDetection, OfflineDetection>();

            kernel.BindAndKeepInstance<IRecentProjectsManager, RecentProjectsManager>();
        }
    }
}