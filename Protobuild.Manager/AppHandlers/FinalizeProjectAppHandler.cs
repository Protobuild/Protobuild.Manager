using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Protobuild.Manager
{
    public class FinalizeProjectAppHandler : IAppHandler
    {
        private readonly RuntimeServer _runtimeServer;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IWorkflowManager _workflowManager;

        public FinalizeProjectAppHandler(RuntimeServer runtimeServer, IWorkflowFactory workflowFactory, IWorkflowManager workflowManager)
        {
            _runtimeServer = runtimeServer;
            _workflowFactory = workflowFactory;
            _workflowManager = workflowManager;
        }

        public void Handle(NameValueCollection parameters)
        {
            var name = parameters["name"];
            var path = parameters["path"];
            var projectFormat = parameters["projectFormat"];

            Directory.CreateDirectory(path);

            var client = new WebClient();
            client.DownloadFile("http://protobuild.org/get", Path.Combine(path, "Protobuild.exe"));

            var startProcess = Process.Start(new ProcessStartInfo(Path.Combine(path, "Protobuild.exe"), "--start \"" + _runtimeServer.Get<string>("templateurl") + "\" \"" + name + "\"")
            {
                WorkingDirectory = path,
                UseShellExecute = false
            });
            if (startProcess == null)
            {
                throw new InvalidOperationException("can't create");
            }
            startProcess.WaitForExit();

            if (projectFormat == "standard" || projectFormat.StartsWith("standard-"))
            {
                // Generate for all selected platforms.
                var platforms =
                    parameters.Keys.OfType<string>().Where(x => x.StartsWith("platform_"))
                        .Select(x => x.Substring(9))
                        .Aggregate((a, b) => a + "," + b);
                var generateProcess =
                    Process.Start(new ProcessStartInfo(Path.Combine(path, "Protobuild.exe"), "--generate " + platforms)
                    {
                        WorkingDirectory = path,
                        UseShellExecute = false
                    });
                if (generateProcess == null)
                {
                    throw new InvalidOperationException("can't generate");
                }
                generateProcess.WaitForExit();

                // Remove the Protobuild.exe file and Build folders.
                Directory.Delete(Path.Combine(path, "Build"), true);
                File.Delete(Path.Combine(path, "Protobuild.exe"));

#if PLATFORM_WINDOWS
                System.Windows.Forms.MessageBox.Show("Your project has been created.");
#endif

                _runtimeServer.Goto("index");
                _workflowManager.AppendWorkflow(_workflowFactory.CreateInitialWorkflow());
            }
            else
            {
                _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(path));
            }
        }
    }
}