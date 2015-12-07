using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class ProjectCreator : IProjectCreator
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IProjectDefaultPath _projectDefaultPath;

        public ProjectCreator(RuntimeServer runtimeServer, IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, IProjectDefaultPath projectDefaultPath)
        {
            _runtimeServer = runtimeServer;
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _projectDefaultPath = projectDefaultPath;
        }

        public void CreateProject(CreateProjectRequest request)
        {
            Task.Run(() => RunInternal(request));
        }

        private async Task RunInternal(CreateProjectRequest request)
        {
            _runtimeServer.Goto("create");

            _projectDefaultPath.SetProjectDefaultPath(new DirectoryInfo(request.Path).Parent.FullName + Path.DirectorySeparatorChar);

            var steps = new List<KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>>();
            var isStandard = request.ProjectFormat.StartsWith("standard");
            var projectFormatId = 
                (request.ProjectFormat != "standard" && request.ProjectFormat != "protobuild") ?
                (isStandard ? request.ProjectFormat.Substring(9) : request.ProjectFormat.Substring(11)) :
                string.Empty;

            steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Create project directory", CreateProjectDirectory));
            steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Download Protobuild", DownloadProtobuild));
            steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Instantiate from base template", InstantiateFromBaseTemplate));
            
            if (request.ProjectFormat != "standard" && request.ProjectFormat != "protobuild")
            {
                var variant =
                    (isStandard
                        ? request.Template.AdditionalStandardProjectVariants
                        : request.Template.AdditionalProtobuildVariants)[projectFormatId];
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Apply '" + variant + "' project format overlay", (x, y) => ApplyProjectFormatOverlay(x, y, projectFormatId)));
            }

            foreach (var v in request.Template.OptionVariants)
            {
                var sv = (isStandard ? v.StandardOptions : v.ProtobuildOptions).First(x => x.ID == request.Parameters[v.ID]);
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                    "Apply '" + v.Name + ": " + sv.Name + "' overlay", (x, y) => ApplyOptionalFormatOverlay(x, y, sv.OverlayPath)));
            }
            
            if (isStandard)
            {
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Resolve packages", ResolvePackages));
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Generate projects", GeneratePlatforms));
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Clean up", CleanUpForStandardProjects));
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Open project", OpenStandardProject));
            }
            else
            {
                steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Open project", OpenProtobuildProject));
            }

            var i = 0;
            foreach (var step in steps)
            {
                _runtimeServer.Set("stepName" + i, step.Key);
                _runtimeServer.Set("stepState" + i, "Pending");
                _runtimeServer.Set("stepMessage" + i, null);
                i++;
            }
            _runtimeServer.Set("stepCount", i);

            i = 0;
            foreach (var step in steps)
            {
                _runtimeServer.Flush();
                _runtimeServer.Set("stepState" + i, "Processing");
                try
                {
                    var i1 = i;
                    await step.Value(request, (a, b) =>
                    {
                        _runtimeServer.Set("stepName" + i1, a);
                        _runtimeServer.Set("stepMessage" + i1, b);
                    });
                    _runtimeServer.Set("stepState" + i, "Passed");
                }
                catch (Exception ex)
                {
                    _runtimeServer.Set("stepState" + i, "Failed");
                    _runtimeServer.Set("stepMessage" + i, ex.ToString());
                }

                i++;
            }
        }

        private async Task CreateProjectDirectory(CreateProjectRequest arg, Action<string, string> update)
        {
            Directory.CreateDirectory(arg.Path);
            await Task.Yield();
        }

        private async Task DownloadProtobuild(CreateProjectRequest arg, Action<string, string> update)
        {
            var client = new WebClient();
            await client.DownloadFileTaskAsync("http://protobuild.org/get", Path.Combine(arg.Path, "Protobuild.exe"));
        }

        private async Task InstantiateFromBaseTemplate(CreateProjectRequest arg, Action<string, string> update)
        {
            var startProcess = Process.Start(new ProcessStartInfo(Path.Combine(arg.Path, "Protobuild.exe"), "--start \"" + arg.Template.TemplateURI + "\" \"" + arg.Name + "\"")
            {
                WorkingDirectory = arg.Path,
                UseShellExecute = false,
                CreateNoWindow = true,
            });
            if (startProcess == null)
            {
                throw new InvalidOperationException("can't create");
            }
            await startProcess.WaitForExitAsync();
        }

        private async Task ApplyProjectFormatOverlay(CreateProjectRequest createProjectRequest, Action<string, string> update, string projectFormatId)
        {
            await Task.Yield();
        }

        private async Task ApplyOptionalFormatOverlay(CreateProjectRequest createProjectRequest, Action<string, string> update, string overlayPath)
        {
            await Task.Yield();
        }

        private async Task ResolvePackages(CreateProjectRequest arg, Action<string, string> update)
        {
            // Generate for all selected platforms.
            var platforms =
                arg.Parameters.Keys.OfType<string>().Where(x => x.StartsWith("platform_"))
                    .Select(x => x.Substring(9))
                    .Aggregate((a, b) => a + "," + b);
            var generateProcess =
                Process.Start(new ProcessStartInfo(Path.Combine(arg.Path, "Protobuild.exe"), "--resolve " + platforms)
                {
                    WorkingDirectory = arg.Path,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                });
            if (generateProcess == null)
            {
                throw new InvalidOperationException("can't generate");
            }
            var lastPlatformRegex = new Regex("^Starting resolution of packages for (?<platform>[A-Za-z0-9]+)\\.\\.\\.$");
            string lastPlatform = null;
            var allowUpdate = true;
            generateProcess.OutputDataReceived += (sender, args) =>
            {
                if (!allowUpdate)
                {
                    return;
                }

                var line = args.Data;
                if (line != null)
                {
                    var match = lastPlatformRegex.Match(line);
                    if (match.Success)
                    {
                        lastPlatform = match.Groups["platform"].Value;
                    }
                }

                if (lastPlatform != null)
                {
                    update("Resolve packages for " + lastPlatform, line);
                }
                else
                {
                    update("Resolve packages", line);
                }
            };
            generateProcess.BeginOutputReadLine();
            generateProcess.EnableRaisingEvents = true;

            await generateProcess.WaitForExitAsync();
            allowUpdate = false;

            update("Resolve packages", null);
        }

        private async Task GeneratePlatforms(CreateProjectRequest arg, Action<string, string> update)
        {
            // Generate for all selected platforms.
            var platforms =
                arg.Parameters.Keys.OfType<string>().Where(x => x.StartsWith("platform_"))
                    .Select(x => x.Substring(9))
                    .Aggregate((a, b) => a + "," + b);
            var generateProcess =
                Process.Start(new ProcessStartInfo(Path.Combine(arg.Path, "Protobuild.exe"), "--no-host-generate --generate " + platforms)
                {
                    WorkingDirectory = arg.Path,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                });
            if (generateProcess == null)
            {
                throw new InvalidOperationException("can't generate");
            }
            var lastPlatformRegex = new Regex("^Starting generation of projects for (?<platform>[A-Za-z0-9]+)$");
            string lastPlatform = null;
            var allowUpdate = true;
            generateProcess.OutputDataReceived += (sender, args) =>
            {
                if (!allowUpdate)
                {
                    return;
                }

                var line = args.Data;
                if (line != null)
                {
                    var match = lastPlatformRegex.Match(line);
                    if (match.Success)
                    {
                        lastPlatform = match.Groups["platform"].Value;
                    }
                }

                if (lastPlatform != null)
                {
                    update("Generate projects for " + lastPlatform, line);
                }
                else
                {
                    update("Generate projects", line);
                }
            };
            generateProcess.BeginOutputReadLine();
            generateProcess.EnableRaisingEvents = true;

            await generateProcess.WaitForExitAsync();
            allowUpdate = false;

            update("Generate projects", null);
        }

        private async Task CleanUpForStandardProjects(CreateProjectRequest arg, Action<string, string> update)
        {
            // Remove the Protobuild.exe file and Build folders.
            Directory.Delete(Path.Combine(arg.Path, "Build"), true);
            File.Delete(Path.Combine(arg.Path, "Protobuild.exe"));

            await Task.Yield();
        }

        private async Task OpenProtobuildProject(CreateProjectRequest arg, Action<string, string> update)
        {
            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(arg.Path));

            await Task.Yield();
        }

        private async Task OpenStandardProject(CreateProjectRequest arg, Action<string, string> update)
        {
#if PLATFORM_WINDOWS
            System.Windows.Forms.MessageBox.Show("Your project has been created.");
#endif

            _runtimeServer.Goto("index");
            _workflowManager.AppendWorkflow(_workflowFactory.CreateInitialWorkflow());

            await Task.Yield();
        }
    }
}
