using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuild.Manager
{
    public class ProjectOpenWorkflow : IWorkflow
    {
        private readonly RuntimeServer _runtimeServer;

        public ProjectOpenWorkflow(RuntimeServer runtimeServer, string path, string name)
        {
            _runtimeServer = runtimeServer;
        }

        public void Run()
        {
            _runtimeServer.Goto("project");
        }
    }
}
