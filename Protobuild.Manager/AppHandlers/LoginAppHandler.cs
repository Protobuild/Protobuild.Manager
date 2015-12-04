using System;
using System.Diagnostics;
using System.Collections.Specialized;

namespace Unearth
{
    public class LoginAppHandler : IAppHandler
    {
        private readonly IWorkflowManager m_WorkflowManager;

        private readonly IWorkflowFactory m_WorkflowFactory;

        private readonly IOfflineDetection m_OfflineDetection;

        public LoginAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, IOfflineDetection offlineDetection)
        {
            this.m_WorkflowManager = workflowManager;
            this.m_WorkflowFactory = workflowFactory;
            this.m_OfflineDetection = offlineDetection;
        }

        public void Handle(NameValueCollection parameters)
        {
            this.m_WorkflowManager.AppendWorkflow(
                this.m_WorkflowFactory.CreatePrereqWorkflow());

            if (this.m_OfflineDetection.Offline)
            {
                this.m_WorkflowManager.AppendWorkflow(
                    this.m_WorkflowFactory.CreateLaunchGameWorkflow());
            }
            else
            {
                this.m_WorkflowManager.AppendWorkflow(
                    this.m_WorkflowFactory.CreateAuthWorkflow(
                        parameters["username"] ?? string.Empty,
                        parameters["password"] ?? string.Empty,
                        parameters["cached"] == "true"));
            }
        }
    }
}

