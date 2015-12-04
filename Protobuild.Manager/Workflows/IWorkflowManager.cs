using System;

namespace Unearth
{
    public interface IWorkflowManager
    {
        void AppendWorkflow(IWorkflow workflow);

        void Start();
    }
}

