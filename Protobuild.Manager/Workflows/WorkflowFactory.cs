using System;

namespace Protobuild.Manager
{
    public class WorkflowFactory : IWorkflowFactory
    {
        private readonly LightweightKernel m_Kernel;

        public WorkflowFactory(LightweightKernel kernel)
        {
            this.m_Kernel = kernel;
        }

        /*

        public IWorkflow CreateAuthWorkflow(string username, string password, bool useCached)
        {
            return new AuthWorkflow(
                this.m_Kernel.Get<RuntimeServer>(),
                this.m_Kernel.Get<IWorkflowManager>(),
                this.m_Kernel.Get<IWorkflowFactory>(),
                username,
                password,
                useCached);
        }

        public IWorkflow CreateLaunchGameWorkflow()
        {
#if PLATFORM_LINUX
            return this.m_Kernel.Get<LaunchGameLinuxWorkflow>();
#elif PLATFORM_MACOS
            return this.m_Kernel.Get<LaunchGameMacOSWorkflow>();
#elif PLATFORM_WINDOWS
            return this.m_Kernel.Get<LaunchGameWindowsWorkflow>();
#endif
        }

        public IWorkflow CreateUpdateGameWorkflow()
        {
            return this.m_Kernel.Get<UpdateGameWorkflow>();
        }

        public IWorkflow CreatePrereqWorkflow()
        {
            return this.m_Kernel.Get<PrereqWorkflow>();
        }

*/
    }
}

