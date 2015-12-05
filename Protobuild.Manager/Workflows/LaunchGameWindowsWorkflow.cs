using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace Protobuild.Manager
{
    /*
    public class LaunchGameWindowsWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IPathProvider m_PathProvider;

        private readonly IUIManager m_UIManager;

        private readonly IExecution m_Execution;

        public LaunchGameWindowsWorkflow(
            RuntimeServer runtimeServer,
            IPathProvider pathProvider,
            IUIManager uiManager,
            IExecution execution)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_PathProvider = pathProvider;
            this.m_UIManager = uiManager;
            this.m_Execution = execution;
        }

        public void Run()
        {
            var gamePath = this.m_PathProvider.GetGamePath();

            this.m_RuntimeServer.Set("working", "Starting game");

            var executable = Path.Combine(gamePath, "UnearthGame.exe");
            if (!File.Exists(executable) && File.Exists(Path.Combine(gamePath, "Unearth.exe")))
            {
                executable = Path.Combine(gamePath, "Unearth.exe");
            }

            if (!File.Exists(executable))
            {
                this.m_RuntimeServer.Set("view", "main");
                this.m_RuntimeServer.Set("error", "Game executable missing??");
                return;
            }

            try
            {
                this.m_Execution.ExecuteApplicationExecutable(executable);
            }
            catch (Exception ex)
            {
                this.m_RuntimeServer.Set("view", "main");
                this.m_RuntimeServer.Set("error", ex.Message);
                return;
            }

            // Give the game some time to start.
            Thread.Sleep(3000);

            this.m_UIManager.Quit();
        }
    }
    */
}

