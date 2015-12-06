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

        public OpenOtherAppHandler(IWorkflowManager workflowManager, IWorkflowFactory workflowFactory)
        {
            _workflowManager = workflowManager;
            _workflowFactory = workflowFactory;
        }

        public void Handle(NameValueCollection parameters)
        {
            string selectedPath = null;

#if PLATFORM_WINDOWS
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Protobuild Module|Protobuild.exe";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.AutoUpgradeEnabled = true;
            ofd.CheckPathExists = true;
            ofd.Title = "Select Protobuild Module";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var fileInfo = new FileInfo(ofd.FileName);
                if (!fileInfo.Exists || fileInfo.Name.ToLowerInvariant() != "Protobuild.exe".ToLowerInvariant())
                {
                    return;
                }
                selectedPath = fileInfo.DirectoryName;
            }
#endif

            if (selectedPath != null)
            {
                _workflowManager.AppendWorkflow(_workflowFactory.CreateProjectOpenWorkflow(selectedPath));
            }
        }
    }
}