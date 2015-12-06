namespace Protobuild.Manager
{
    public class ProjectNewWorkflow : IWorkflow
    {
        private readonly RuntimeServer _runtimeServer;

        public ProjectNewWorkflow(RuntimeServer runtimeServer)
        {
            _runtimeServer = runtimeServer;
        }

        public void Run()
        {
            _runtimeServer.Goto("selecttemplate");
        }
    }
}