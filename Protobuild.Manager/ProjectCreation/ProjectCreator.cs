using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Protobuild.Manager
{
    public class ProjectCreator : IProjectCreator
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IProjectDefaultPath _projectDefaultPath;
        private readonly IProjectOverlay _projectOverlay;
		private readonly IExecution _execution;
		private readonly IProcessLog _processLog;
        private readonly IConfigManager _configManager;

        public ProjectCreator(RuntimeServer runtimeServer, IWorkflowManager workflowManager,
            IWorkflowFactory workflowFactory, IProjectDefaultPath projectDefaultPath,
			IProjectOverlay projectOverlay, IExecution execution, IProcessLog processLog,
            IConfigManager configManager)
        {
            _runtimeServer = runtimeServer;
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _projectDefaultPath = projectDefaultPath;
            _projectOverlay = projectOverlay;
			_execution = execution;
			_processLog = processLog;
            _configManager = configManager;
        }

        public void CreateProject(CreateProjectRequest request)
        {
            Task.Run(() => RunInternal(request));
        }

        private async Task RunInternal(CreateProjectRequest request)
        {
            _runtimeServer.Goto("create");

            try
            {
                _projectDefaultPath.SetProjectDefaultPath(new DirectoryInfo(request.Path).Parent.FullName +
                                                          Path.DirectorySeparatorChar);

                var steps = new List<KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>>();
                var isStandard = request.ProjectFormat.StartsWith("standard");
                var projectFormatId =
                    (request.ProjectFormat != "standard" && request.ProjectFormat != "protobuild")
                        ? (isStandard ? request.ProjectFormat.Substring(9) : request.ProjectFormat.Substring(11))
                        : string.Empty;

                steps.Add(
                    new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                        "Create project directory", CreateProjectDirectory));
                steps.Add(
                    new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                        "Copy Protobuild", CopyProtobuild));
                steps.Add(
                    new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                        "Create project from template", InstantiateFromBaseTemplate));

                if (request.ProjectFormat != "standard" && request.ProjectFormat != "protobuild")
                {
                    var variant =
                        (isStandard
                            ? request.Template.AdditionalStandardProjectVariants
                            : request.Template.AdditionalProtobuildVariants)[projectFormatId];
                    steps.Add(
                        new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                            "Apply '" + variant.Name + "' project format overlay",
                            (x, y) => ApplyOverlay(x, y, variant.OverlayPath)));
                }

                foreach (var v in request.Template.OptionVariants)
                {
                    var sv =
                        (isStandard ? v.StandardOptions : v.ProtobuildOptions).FirstOrDefault(
                            x => x.ID == request.Parameters[v.ID]);
                    if (sv != null && sv.OverlayPath != null)
                    {
                        steps.Add(new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                            "Apply '" + v.Name + ": " + sv.Name + "' overlay", (x, y) => ApplyOverlay(x, y, sv.OverlayPath)));
                    }
                }

                if (isStandard)
                {
                    steps.Add(
                        new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                            "Resolve packages", ResolvePackages));
                    steps.Add(
                        new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                            "Generate projects", GeneratePlatforms));
                    steps.Add(
                        new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Clean up",
                            CleanUpForStandardProjects));
                }
                else
                {
                    steps.Add(
                        new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>(
                            "Configure services", ConfigureServices));
                }

                steps.Add(
                    new KeyValuePair<string, Func<CreateProjectRequest, Action<string, string>, Task>>("Open project",
                        OpenProject));

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
            catch (Exception ex)
            {
                _runtimeServer.Set("stepState0", "Error");
                _runtimeServer.Set("stepName0", ex.Message);
                _runtimeServer.Set("stepMessage0", ex.StackTrace);
                _runtimeServer.Set("stepCount", 1);
            }
        }

        private async Task CreateProjectDirectory(CreateProjectRequest arg, Action<string, string> update)
        {
            Directory.CreateDirectory(arg.Path);
            await Task.Yield();
        }

        private async Task CopyProtobuild(CreateProjectRequest arg, Action<string, string> update)
        {
            var client = new WebClient();
            client.Headers.Add("User-Agent", "Protobuild Manager/1.0.0");

            var downloadNewVersion = false;
            string targetHash = null;
            var cachedPath = Path.Combine(_configManager.GetBasePath(), "Protobuild.exe");
            var cachedHashPath = Path.Combine(_configManager.GetBasePath(), "Protobuild.exe.hash");
            if (!File.Exists(cachedPath))
            {
                downloadNewVersion = true;
            }
            else
            {
                try
                {
                    string cachedHash;
                    using (var reader = new StreamReader(cachedHashPath))
                    {
                        cachedHash = reader.ReadToEnd().Trim();
                    }

                    update("Copy Protobuild", "Checking for new versions...");

                    // Try to check with GitHub to see if the master branches points somewhere else.
                    var branchJson = await client.DownloadStringTaskAsync("https://api.github.com/repos/hach-que/Protobuild/branches/master");
                    var serializer = new JavaScriptSerializer();
                    var branchInfo = serializer.Deserialize<Dictionary<string, object>>(branchJson);
                    var commitInfo = (Dictionary<string, object>) branchInfo["commit"];
                    targetHash = (string) commitInfo["sha"];

                    if (cachedHash != targetHash)
                    {
                        downloadNewVersion = true;
                    }
                }
                catch (Exception)
                {
                    // Don't update, use the existing version.
                }
            }

            if (downloadNewVersion)
            {
                update("Copy Protobuild", "Downloading new version...");
                await client.DownloadFileTaskAsync("http://protobuild.org/get", cachedPath);

                using (var writer = new StreamWriter(cachedHashPath))
                {
                    writer.Write(targetHash);
                }
            }

            update("Copy Protobuild", null);
            File.Copy(cachedPath, Path.Combine(arg.Path, "Protobuild.exe"));
        }

        private async Task InstantiateFromBaseTemplate(CreateProjectRequest arg, Action<string, string> update)
        {
			var startProcess = _execution.ExecuteConsoleExecutable(
				Path.Combine(arg.Path, "Protobuild.exe"), 
				"--no-generate --start \"" + arg.Template.TemplateURI + "\" \"" + arg.Name + "\"",
				si =>
				{
					si.WorkingDirectory = arg.Path;
					si.UseShellExecute = false;
					si.CreateNoWindow = true;
					si.RedirectStandardOutput = true;
					si.RedirectStandardError = true;
				},
                _processLog.PrepareForAttachToProcess);
            if (startProcess == null)
            {
                throw new InvalidOperationException("can't create");
			}
			var allowUpdate = true;
			startProcess.OutputDataReceived += (sender, args) =>
			{
				if (!allowUpdate)
				{
					return;
				}

				var line = args.Data;
				update("Create project from template", line);
			};
			_processLog.AttachToProcess(startProcess);
			startProcess.EnableRaisingEvents = true;

			await startProcess.WaitForExitAsync();
			allowUpdate = false;

			if (startProcess.ExitCode != 0)
			{
				throw new InvalidOperationException("Protobuild exited with a non-zero exit code!");
			}

			update("Create project from template", null);
        }
        
        private async Task ApplyOverlay(CreateProjectRequest createProjectRequest, Action<string, string> update, string overlayPath)
        {
            _projectOverlay.ApplyProjectTemplateOverlay(overlayPath, createProjectRequest.Path, createProjectRequest.Name);
            await Task.Yield();
        }

        private async Task ResolvePackages(CreateProjectRequest arg, Action<string, string> update)
        {
            // Resolve packages for all selected platforms.
            var platforms =
                arg.Parameters.Keys.OfType<string>().Where(x => x.StartsWith("platform_"))
                    .Select(x => x.Substring(9))
					.Aggregate((a, b) => a + "," + b);
			var generateProcess = _execution.ExecuteConsoleExecutable(
				Path.Combine(arg.Path, "Protobuild.exe"), 
				"--resolve " + platforms,
				si =>
				{
					si.WorkingDirectory = arg.Path;
					si.UseShellExecute = false;
					si.CreateNoWindow = true;
					si.RedirectStandardOutput = true;
					si.RedirectStandardError = true;
				},
                _processLog.PrepareForAttachToProcess);
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
			_processLog.AttachToProcess(generateProcess);
            generateProcess.EnableRaisingEvents = true;

            await generateProcess.WaitForExitAsync();
            allowUpdate = false;

            if (generateProcess.ExitCode != 0)
            {
                throw new InvalidOperationException("Protobuild exited with a non-zero exit code!");
            }

            update("Resolve packages", null);
        }

        private async Task GeneratePlatforms(CreateProjectRequest arg, Action<string, string> update)
        {
            // Calculate service invocation from arguments.
            var servicesSpec = string.Empty;
            var isStandard = arg.ProjectFormat.StartsWith("standard");
            foreach (var v in arg.Template.OptionVariants)
            {
                var sv =
                    (isStandard ? v.StandardOptions : v.ProtobuildOptions).First(
                        x => x.ID == arg.Parameters[v.ID]);
                foreach (var enable in sv.EnableServices)
                {
                    servicesSpec += " --enable " + enable;
                }
                foreach (var enable in sv.DisableServices)
                {
                    servicesSpec += " --disable " + enable;
                }
            }

            // Generate for all selected platforms.
            var platforms =
                arg.Parameters.Keys.OfType<string>().Where(x => x.StartsWith("platform_"))
                    .Select(x => x.Substring(9))
					.Aggregate((a, b) => a + "," + b);
			var generateProcess = _execution.ExecuteConsoleExecutable(
				Path.Combine(arg.Path, "Protobuild.exe"), 
				"--no-host-generate --no-resolve --generate " + platforms + servicesSpec,
				si =>
				{
					si.WorkingDirectory = arg.Path;
					si.UseShellExecute = false;
					si.CreateNoWindow = true;
					si.RedirectStandardOutput = true;
					si.RedirectStandardError = true;
				},
                _processLog.PrepareForAttachToProcess);
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
			_processLog.AttachToProcess(generateProcess);
            generateProcess.EnableRaisingEvents = true;

            await generateProcess.WaitForExitAsync();
            allowUpdate = false;

            if (generateProcess.ExitCode != 0)
            {
                throw new InvalidOperationException("Protobuild exited with a non-zero exit code!");
            }

            update("Generate projects", null);
        }

        private async Task ConfigureServices(CreateProjectRequest arg, Action<string, string> update)
        {
            // Calculate service invocation from arguments.
            var enable = new List<string>();
            var disable = new List<string>();
            var isStandard = arg.ProjectFormat.StartsWith("standard");
            foreach (var v in arg.Template.OptionVariants)
            {
                var sv =
                    (isStandard ? v.StandardOptions : v.ProtobuildOptions).First(
                        x => x.ID == arg.Parameters[v.ID]);
                enable = sv.EnableServices.ToList();
                disable = sv.DisableServices.ToList();
            }

            var conflicts = string.Empty;
            var requires = string.Empty;

            if (enable.Count > 0)
            {
                requires = "<Requires>" + enable.DefaultIfEmpty(string.Empty).Aggregate((a, b) => a + "," + b) +
                           @"</Requires>";
            }

            if (disable.Count > 0)
            {
                conflicts = "<Conflicts>" + disable.DefaultIfEmpty(string.Empty).Aggregate((a, b) => a + "," + b) +
                           @"</Conflicts>";
            }

            if (requires == string.Empty && conflicts == string.Empty)
            {
                return;
            }

            using (var writer = new StreamWriter(Path.Combine(arg.Path, "Build", "Projects", "_Services.definition")))
            {
                await writer.WriteLineAsync(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ExternalProject Name=""" + arg.Name + @"_Services"">
  <!--

  Normally services are selected from the command line by appending -enable
  or -disable when generating projects, or you would add a dependency to the
  service on your application project.

  However, we don't have control over how the project is generated later when
  using the project creator, and we don't know an appropriate project definition
  to add a dependency to.  So instead we create an external project here that
  depends on a service configure that then requires and conflicts with the
  specified services to get the intended result.

  -->
  <Dependencies>
    <Uses Name=""_ServiceConfiguration"" />
  </Dependencies>
  <Services>
    <Service Name=""_ServiceConfiguration"">
      " + conflicts + requires + @"
    </Service>
  </Services>
</ExternalProject>
".Trim());
            }
        }

        private async Task CleanUpForStandardProjects(CreateProjectRequest arg, Action<string, string> update)
        {
            // Remove the Protobuild.exe file and Build folders.
            Directory.Delete(Path.Combine(arg.Path, "Build"), true);
            File.Delete(Path.Combine(arg.Path, "Protobuild.exe"));
			foreach (var file in new DirectoryInfo(arg.Path).GetFiles("*.speccache").ToList())
			{
				File.Delete(file.FullName);
			}

            await Task.Yield();
        }

        private async Task OpenProject(CreateProjectRequest arg, Action<string, string> update)
        {
            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(arg.Path));

            await Task.Yield();
        }
    }
}
