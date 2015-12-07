﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class ProjectOpenWorkflow : IWorkflow
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IRecentProjectsManager _recentProjectsManager;
        private readonly string _path;
        private readonly IProtobuildHostingEngine _protobuildHostingEngine;

        public ProjectOpenWorkflow(RuntimeServer runtimeServer, IProtobuildHostingEngine protobuildHostingEngine,
            IRecentProjectsManager recentProjectsManager, string path)
        {
            _runtimeServer = runtimeServer;
            _recentProjectsManager = recentProjectsManager;
            _path = path;

            if (File.Exists(Path.Combine(path, "Protobuild.exe")))
            {
                ModuleHost = protobuildHostingEngine.LoadModule(path);
            }
            else
            {
                IsStandardProject = true;
            }
        }

        public bool IsStandardProject { get; set; }

        public ModuleHost ModuleHost { get; set; }

        public void Run()
        {
            if (IsStandardProject)
            {
                var directory = new DirectoryInfo(_path);
                string prefix = null;
                var platforms = new List<string>();
                foreach (var file in directory.GetFiles("*.sln"))
                {
                    var basename = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                    var lastDot = basename.LastIndexOf(".");
                    if (lastDot == -1)
                    {
                        continue;
                    }
                    prefix = basename.Substring(0, lastDot);
                    platforms.Add(basename.Substring(lastDot + 1));
                }

                if (prefix == null)
                {
                    _runtimeServer.Set("loadedModuleName", "No Solution Found!");
                    _runtimeServer.Set("loadedModulePath", null);
                }
                else
                {
                    _runtimeServer.Set("loadedModuleName", prefix);
                    _runtimeServer.Set("loadedModulePath", _path);
                }

                var i = 0;
                foreach (var platform in platforms)
                {
                    _runtimeServer.Set("supportedPlatform" + i, platform);
                    i++;
                }
                _runtimeServer.Set("supportedPlatformCount", platforms.Count);
                _runtimeServer.Set("isStandard", true);
            }
            else
            {
                _runtimeServer.Set("loadedModuleName", ModuleHost.LoadedModule.Name);
                _runtimeServer.Set("loadedModulePath", ModuleHost.LoadedModule.Path);
                _runtimeServer.Set("isStandard", false);
                _runtimeServer.Set("supportedPlatformCount", 0);
            }

            _recentProjectsManager.AddEntry(
                _runtimeServer.Get<string>("loadedModuleName"),
                _runtimeServer.Get<string>("loadedModulePath"));

            _runtimeServer.Goto("project");
        }
    }
}
