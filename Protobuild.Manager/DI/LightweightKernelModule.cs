using System;
using System.Reflection;
using System.IO;

namespace Protobuild.Manager
{
    public static class LightweightKernelModule
    {
        public static void BindCommon(this LightweightKernel kernel)
        {
            var brandingPath = Path.Combine(Environment.CurrentDirectory, "Branding.xml");
            if (File.Exists(brandingPath))
            {
                kernel.BindAndKeepInstance<IBrandingEngine, ExternalBrandingEngine>();
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
            kernel.BindAndKeepInstance<IStartup, Startup>();
            kernel.BindAndKeepInstance<IProtobuildHostingEngine, ProtobuildHostingEngine>();

            kernel.BindAndKeepInstance<IRecentProjectsManager, RecentProjectsManager>();

            var branding = kernel.Get<IBrandingEngine>();
            if (branding.TemplateSource == "builtin")
            {
                kernel.BindAndKeepInstance<ITemplateSource, BuiltinTemplateSource>();
            }
            else if (branding.TemplateSource == "online")
            {
                throw new NotSupportedException();
            }
            else if (branding.TemplateSource.StartsWith("dir:"))
            {
                kernel.BindAndKeepInstance<ITemplateSource, OnDiskTemplateSource>();
            }

            kernel.BindAndKeepInstance<IProjectCreator, ProjectCreator>();
            kernel.BindAndKeepInstance<IProjectDefaultPath, ProjectDefaultPath>();
            kernel.BindAndKeepInstance<IProjectOverlay, ProjectOverlay>();
			kernel.BindAndKeepInstance<IProcessLog, RuntimeServerProcessLog>();
            kernel.BindAndKeepInstance<IProtobuildProvider, ProtobuildProvider>();
            kernel.BindAndKeepInstance<IAddinPackageDownload, AddinPackageDownload>();
        }
    }
}