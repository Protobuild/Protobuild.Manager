using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Reflection;

namespace Protobuild.Manager
{
    public class ProjectOpenWorkflow : IWorkflow
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IRecentProjectsManager _recentProjectsManager;
        private readonly string _path;
        private readonly IProtobuildHostingEngine _protobuildHostingEngine;

        public ProjectOpenWorkflow(
			RuntimeServer runtimeServer, 
			IProtobuildHostingEngine protobuildHostingEngine,
            IRecentProjectsManager recentProjectsManager, 
			IUIManager uiManager,
			string path)
        {
            _runtimeServer = runtimeServer;
            _recentProjectsManager = recentProjectsManager;
            _path = path;

            if (File.Exists(Path.Combine(path, "Protobuild.exe")))
            {
				try
				{
                	ModuleHost = protobuildHostingEngine.LoadModule(path);
                }
                catch (FileLoadException)
                {
                    if (uiManager.AskToRepairCorruptProtobuild())
                    {
                        var client = new WebClient();
                        File.Delete(Path.Combine(path, "Protobuild.exe"));
                        client.DownloadFile("http://protobuild.org/get", Path.Combine(path, "Protobuild.exe"));

                        try
                        {
                            ModuleHost = protobuildHostingEngine.LoadModule(path);
                        }
                        catch (FileLoadException)
                        {
                            uiManager.FailedToRepairCorruptProtobuild();
                            IsStandardProject = true;
                        }
                        catch (BadImageFormatException)
                        {
                            uiManager.FailedToRepairCorruptProtobuild();
                            IsStandardProject = true;
                        }
                        catch (TargetInvocationException)
                        {
                            uiManager.UnableToLoadModule();
                            IsStandardProject = true;
                        }
                    }
                    else
                    {
                        IsStandardProject = true;
                    }
                }
                catch (BadImageFormatException)
				{
					if (uiManager.AskToRepairCorruptProtobuild())
					{
						var client = new WebClient();
						File.Delete(Path.Combine(path, "Protobuild.exe"));
						client.DownloadFile("http://protobuild.org/get", Path.Combine(path, "Protobuild.exe"));

						try
						{
							ModuleHost = protobuildHostingEngine.LoadModule(path);
                        }
                        catch (FileLoadException)
                        {
                            uiManager.FailedToRepairCorruptProtobuild();
                            IsStandardProject = true;
                        }
                        catch (BadImageFormatException)
						{
							uiManager.FailedToRepairCorruptProtobuild();
							IsStandardProject = true;
						}
						catch (TargetInvocationException)
						{
							uiManager.UnableToLoadModule();
							IsStandardProject = true;
						}
					}
					else
					{
						IsStandardProject = true;
					}
				}
				catch (TargetInvocationException)
				{
					uiManager.UnableToLoadModule();
					IsStandardProject = true;
				}
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
                if (!Directory.Exists(_path))
                {
                    var result = System.Windows.Forms.MessageBox.Show("Directory does not exist.  Remove from history?", _path, System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        _recentProjectsManager.RemoveEntry(_path);
                        _runtimeServer.Goto("index");
                    }
                    return;
                }
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

            if (Path.DirectorySeparatorChar == '/')
            {
                if (Directory.Exists("/Library"))
                {
                    _runtimeServer.Set("hostPlatform", "MacOS");
                }
                else
                {
                    _runtimeServer.Set("hostPlatform", "Linux");
                }
            }
            else
            {
                _runtimeServer.Set("hostPlatform", "Windows");
            }

            _runtimeServer.Goto("project");
        }
    }
}
