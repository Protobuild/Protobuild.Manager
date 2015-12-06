using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
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

            Directory.CreateDirectory(path);

            var client = new WebClient();
            client.DownloadFile("http://protobuild.org/get", Path.Combine(path, "Protobuild.exe"));

            var syncProcess = Process.Start(new ProcessStartInfo(Path.Combine(path, "Protobuild.exe"), "--start \"" + _runtimeServer.Get<string>("templateurl") + "\" \"" + name + "\"")
            {
                WorkingDirectory = path,
                UseShellExecute = false
            });
            if (syncProcess == null)
            {
                throw new InvalidOperationException("can't create");
            }
            syncProcess.WaitForExit();

            _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(path));
        }
    }
}