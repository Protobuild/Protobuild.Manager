using System;

namespace Protobuild.Manager
{
    /*
    public class UpdateGameWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IPathProvider m_PathProvider;

        private readonly IWorkflowFactory m_WorkflowFactory;

        private readonly IWorkflowManager m_WorkflowManager;

        private readonly IProgressRenderer m_ProgressRenderer;

        public UpdateGameWorkflow(
            RuntimeServer runtimeServer,
            IPathProvider pathProvider,
            IWorkflowManager workflowManager,
            IWorkflowFactory workflowFactory,
            IProgressRenderer progressRenderer)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_PathProvider = pathProvider;
            this.m_WorkflowManager = workflowManager;
            this.m_WorkflowFactory = workflowFactory;
            this.m_ProgressRenderer = progressRenderer;
        }

        public void Run()
        {
            /*
            var gamePath = this.m_PathProvider.GetGamePath();

            string username = string.Empty;
            string phid = string.Empty;
            string certificate = string.Empty;

            ConfigManager.GetSavedConfig(
                ref username,
                ref phid,
                ref certificate);

            var gameUpdater = new GameUpdater(
                username,
                certificate,
#if PLATFORM_LINUX
                "Unearth-Linux.zip",
#elif PLATFORM_WINDOWS
                "Unearth-Windows.zip",
#else
                "Unearth-MacOS.zip",
#endif
                gamePath);

            var updated = false;
            try
            {
                updated = gameUpdater.Update(this.OnStatus, this.OnFail, this.OnUpdateProgress);
            }
            catch (ConduitException ex)
            {
                if (ex.Message.Contains("Your client or server clock may not be set correctly."))
                {
                    this.m_RuntimeServer.Set("view", "main");
                    this.m_RuntimeServer.Set("error", "Computer clock or timezone not set correctly");
                }
                else
                {
                    this.m_RuntimeServer.Set("view", "main");
                    this.m_RuntimeServer.Set("error", ex.Message);
                }

                ErrorLog.Log(ex);

                return;
            }

            if (!updated)
            {
                return;
            }

            this.m_WorkflowManager.AppendWorkflow(
                this.m_WorkflowFactory.CreateLaunchGameWorkflow());

        }

        private void OnStatus(string status)
        {
            this.m_RuntimeServer.Set("working", status);
        }

        private void OnFail(string error)
        {
            this.m_RuntimeServer.Set("progressAmount", null);
            this.m_RuntimeServer.Set("progressEta", null);
            this.m_RuntimeServer.Set("view", "main");
            this.m_RuntimeServer.Set("error", error);
        }

        private void OnUpdateProgress(float progress, TimeSpan eta)
        {
            this.m_ProgressRenderer.Update(progress, eta);
        }*/
}

