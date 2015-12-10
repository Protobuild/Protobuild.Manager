using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Protobuild.Manager
{
    internal class Startup : IStartup
    {
        private readonly InitialWorkflow m_InitialWorkflow;

        private readonly LightweightKernel _kernel;
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IUIManager m_UIManager;
        private readonly IIDEProjectTemplateSync _ideProjectTemplateSync;

        private readonly IWorkflowManager m_WorkflowManager;

        internal Startup(
            LightweightKernel kernel,
            RuntimeServer runtimeServer,
            IWorkflowManager workflowManager,
            InitialWorkflow initialWorkflow,
            IUIManager uiManager,
            IIDEProjectTemplateSync ideProjectTemplateSync)
        {
            _kernel = kernel;
            this.m_RuntimeServer = runtimeServer;
            this.m_WorkflowManager = workflowManager;
            this.m_InitialWorkflow = initialWorkflow;
            this.m_UIManager = uiManager;
            _ideProjectTemplateSync = ideProjectTemplateSync;
        }

        public void Start(string[] args)
        {
            if (args == null)
            {
                args = new string[0];
            }

            this.m_RuntimeServer.Start();

            if (args.Length == 2)
            {
                // The application is being initialised from an external source (like Visual Studio).
                // Set up the runtime server state and jump straight to the specified workflow.
                var workflowType =
                    typeof (Startup).Assembly.GetTypes()
                        .First(x => x.Name == args[0] && typeof (IWorkflow).IsAssignableFrom(x));
                this.m_WorkflowManager.AppendWorkflow((IWorkflow) _kernel.Get(workflowType));
                var serializer = new JavaScriptSerializer();
                var workflowState = serializer.Deserialize<Dictionary<string, object>>(Encoding.ASCII.GetString(Convert.FromBase64String(args[1])));
                foreach (var kv in workflowState)
                {
                    this.m_RuntimeServer.Set(kv.Key, kv.Value);
                }
            }
            else
            {
                this.m_WorkflowManager.AppendWorkflow(this.m_InitialWorkflow);
            }

            _ideProjectTemplateSync.Sync();

            this.m_WorkflowManager.Start();
            this.m_UIManager.Run();
        }
    }
}