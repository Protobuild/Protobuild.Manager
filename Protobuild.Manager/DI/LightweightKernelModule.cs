namespace Unearth
{
    public static class LightweightKernelModule
    {
        public static void BindCommon(this LightweightKernel kernel)
        {
            kernel.BindAndKeepInstance<RuntimeServer, RuntimeServer>();
            kernel.BindAndKeepInstance<IChannelLoader, ChannelLoader>();
            kernel.BindAndKeepInstance<IWorkflowManager, WorkflowManager>();
            kernel.BindAndKeepInstance<IAppHandlerManager, AppHandlerManager>();
            kernel.BindAndKeepInstance<IProgressRenderer, ProgressRenderer>();
            kernel.BindAndKeepInstance<IWorkflowFactory, WorkflowFactory>();
            kernel.BindAndKeepInstance<IPathProvider, PathProvider>();
            kernel.BindAndKeepInstance<INewsLoader, NewsLoader>();
            kernel.BindAndKeepInstance<ILauncherSelfUpdate, LauncherSelfUpdate>();
            kernel.BindAndKeepInstance<IStartup, Startup>();
            kernel.BindAndKeepInstance<IOfflineDetection, OfflineDetection>();
        }
    }
}