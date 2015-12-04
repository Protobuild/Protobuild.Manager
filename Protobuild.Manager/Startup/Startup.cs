namespace Unearth
{
    public class Startup : IStartup
    {
        private readonly InitialWorkflow m_InitialWorkflow;

        private readonly LauncherSelfUpdate m_LauncherSelfUpdate;

        private readonly NewsLoader m_NewsLoader;

        private readonly RuntimeServer m_RuntimeServer;

        private readonly IUIManager m_UIManager;

        private readonly IWorkflowManager m_WorkflowManager;

        public Startup(
            RuntimeServer runtimeServer,
            NewsLoader newsLoader,
            LauncherSelfUpdate launcherSelfUpdate,
            IWorkflowManager workflowManager,
            InitialWorkflow initialWorkflow,
            IUIManager uiManager)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_NewsLoader = newsLoader;
            this.m_LauncherSelfUpdate = launcherSelfUpdate;
            this.m_WorkflowManager = workflowManager;
            this.m_InitialWorkflow = initialWorkflow;
            this.m_UIManager = uiManager;
        }

        public void Start()
        {
            this.m_RuntimeServer.Start();
            this.m_NewsLoader.LoadNews();
            this.m_LauncherSelfUpdate.StartCheck();
            this.m_WorkflowManager.AppendWorkflow(this.m_InitialWorkflow);
            this.m_WorkflowManager.Start();
            this.m_UIManager.Run();
        }
    }
}