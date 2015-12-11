using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;

namespace Protobuild.Manager
{
    public class OpenOtherAppHandler : IAppHandler
    {
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IUIManager _uiManager;

        public OpenOtherAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory, IUIManager uiManager)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
            _uiManager = uiManager;
        }

        public void Handle(NameValueCollection parameters)
        {
            var selectedPath = _uiManager.SelectExistingProject();

            if (selectedPath != null)
            {
                _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(selectedPath));
            }
        }
    }
}