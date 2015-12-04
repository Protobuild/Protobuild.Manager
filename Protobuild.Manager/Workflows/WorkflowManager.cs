using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;

namespace Unearth
{
    public class WorkflowManager : IWorkflowManager
    {
        private IWorkflow m_CurrentWorkflow;

        private ConcurrentQueue<IWorkflow> m_PendingWorkflows = new ConcurrentQueue<IWorkflow>();

        public void AppendWorkflow(IWorkflow workflow)
        {
            this.m_PendingWorkflows.Enqueue(workflow);
        }

        public void Start()
        {
            var thread = new Thread(this.Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                IWorkflow workflow;

                if (!this.m_PendingWorkflows.TryDequeue(out workflow))
                {
                    Thread.Sleep(10);
                    continue;
                }

                workflow.Run();
            }
        }
    }
}

