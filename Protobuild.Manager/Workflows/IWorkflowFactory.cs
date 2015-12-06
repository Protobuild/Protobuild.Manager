using System;

namespace Protobuild.Manager
{
    public interface IWorkflowFactory
    {
        /*IWorkflow CreateAuthWorkflow(string username, string password, bool useCached);

        IWorkflow CreateUpdateGameWorkflow();

        IWorkflow CreateLaunchGameWorkflow();

        IWorkflow CreatePrereqWorkflow();*/

        IWorkflow CreateProjectOpenWorkflow(string modulePath);

        IWorkflow CreateInitialWorkflow();

        IWorkflow CreateProjectNewWorkflow();

        IWorkflow CreateProjectNamingWorkflow();
    }
}

