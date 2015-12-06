using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class ProjectOpenWorkflow : IWorkflow
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IRecentProjectsManager _recentProjectsManager;
        private readonly IProtobuildHostingEngine _protobuildHostingEngine;

        public ProjectOpenWorkflow(RuntimeServer runtimeServer, IProtobuildHostingEngine protobuildHostingEngine,
            IRecentProjectsManager recentProjectsManager, string path)
        {
            _runtimeServer = runtimeServer;
            _recentProjectsManager = recentProjectsManager;
            ModuleHost = protobuildHostingEngine.LoadModule(path);
        }

        public ModuleHost ModuleHost { get; set; }

        public void Run()
        {
            _recentProjectsManager.AddEntry(
                ModuleHost.LoadedModule.Name,
                ModuleHost.LoadedModule.Path);

            _runtimeServer.Set("loadedModuleName", ModuleHost.LoadedModule.Name);
            _runtimeServer.Set("loadedModulePath", ModuleHost.LoadedModule.Path);

            _runtimeServer.Goto("project");
        }
    }
}
