using System;

namespace Protobuild.Manager
{
    public interface IWorkflowManager
    {
        void AppendWorkflow(IWorkflow workflow);

        void Start();
    }
}

