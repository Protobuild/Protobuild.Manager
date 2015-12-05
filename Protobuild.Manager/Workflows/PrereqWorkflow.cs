namespace Protobuild.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    #if FALSE

    public class PrereqWorkflow : IWorkflow
    {
        private readonly List<IPrerequisiteCheck> m_Checks;

        private readonly RuntimeServer m_RuntimeServer;

        public PrereqWorkflow(
            LightweightKernel kernel,
            RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_Checks = (from type in typeof(PrereqWorkflow).Assembly.GetTypes()
                             where typeof(IPrerequisiteCheck).IsAssignableFrom(type)
                             where !type.IsAbstract && !type.IsInterface
                             select (IPrerequisiteCheck)kernel.Get(type)).ToList();
        }

        public void Run()
        {
            var checks = ConfigManager.GetPrerequisiteChecksNotCompleted(this.m_Checks);

            if (checks.Count == 0)
            {
                return;
            }

            for (var i = 0; i < checks.Count; i++)
            {
                var check = checks[i];
                this.m_RuntimeServer.Set("prereqName" + i, check.Name);
                this.m_RuntimeServer.Set("prereqStatus" + i, check.Status.ToString());
                this.m_RuntimeServer.Set("prereqMessage" + i, check.Message);
            }

            this.m_RuntimeServer.Set("prereqCount", checks.Count);
            this.m_RuntimeServer.Set("view", "prereq");

            for (int i = 0; i < checks.Count; i++)
            {
                var check = checks[i];

                EventHandler handler = (sender, args) =>
                {
                    this.m_RuntimeServer.Set("prereqName" + i, check.Name);
                    this.m_RuntimeServer.Set("prereqStatus" + i, check.Status.ToString());
                    this.m_RuntimeServer.Set("prereqMessage" + i, check.Message);
                };

                check.StatusChanged += handler;
                check.Check();
                check.StatusChanged -= handler;

                if (check.Status == PrerequisiteCheckStatus.Passed)
                {
                    ConfigManager.MarkPrerequisiteAsPassed(check.ID);
                }
            }

            Thread.Sleep(2500);
        }
    }

    #endif
}