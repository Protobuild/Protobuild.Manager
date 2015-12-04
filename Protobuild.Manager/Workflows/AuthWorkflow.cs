using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace Unearth
{
    public class AuthWorkflow : IWorkflow
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IWorkflowManager m_WorkflowManager;

        private readonly IWorkflowFactory m_WorkflowFactory;

        private readonly string m_Username;

        private readonly string m_Password;

        private readonly bool m_UseCached;

        public AuthWorkflow(
            RuntimeServer runtimeServer, 
            IWorkflowManager workflowManager,
            IWorkflowFactory workflowFactory,
            string username, 
            string password, 
            bool useCached)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_WorkflowManager = workflowManager;
            this.m_WorkflowFactory = workflowFactory;
            this.m_Username = username;
            this.m_Password = password;
            this.m_UseCached = useCached;
        }

        public void Run()
        {
            this.m_RuntimeServer.Set("view", "launching");

            var resultUsername = string.Empty;
            var phid = string.Empty;
            var certificate = string.Empty;
            var canRead = false;

            canRead = ConfigManager.GetSavedConfig(
                ref resultUsername,
                ref phid,
                ref certificate);

            var nameValueCollection = new NameValueCollection();
            var client = new WebClient();
            byte[] data;
            string[] results;

            if (!this.m_UseCached || !canRead)
            {
                nameValueCollection.Add("username", this.m_Username);
                nameValueCollection.Add("password", this.m_Password);

                this.m_RuntimeServer.Set("welcome", "Logging in");

                try
                {
                    data = client.UploadValues(
                        UrlConfig.BASE + "/auth",
                        nameValueCollection);
                }
                catch (WebException ex)
                {
                    this.m_RuntimeServer.Set("view", "main");
                    this.m_RuntimeServer.Set("error", "Server error on login");
                    ErrorLog.Log(ex);
                    return;
                }

                results = Encoding.ASCII.GetString(data).Split(new[] {"\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                if (results[0] == "fail")
                {
                    this.m_RuntimeServer.Set("view", "main");
                    this.m_RuntimeServer.Set("error", results[1]);
                    return;
                }

                if (results[0] != "success")
                {
                    this.m_RuntimeServer.Set("view", "main");
                    this.m_RuntimeServer.Set("error", "Unknown error");
                    return;
                }

                // results[1] used to be first name (server returns username here as well for old launchers)
                phid = results[2];
                resultUsername = results[3];
                certificate = results[4];
            }

            this.m_RuntimeServer.Set("welcome", "Welcome " + resultUsername);

            this.m_RuntimeServer.Set("working", "Checking license");

            nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("userPHID", phid);

            try
            {
                data = client.UploadValues(
                    UrlConfig.BASE + "/license",
                    nameValueCollection);
            }
            catch (WebException ex)
            {
                this.m_RuntimeServer.Set("view", "main");
                this.m_RuntimeServer.Set("error", "Server error on license check");
                ErrorLog.Log(ex);
                return;
            }

            results = Encoding.ASCII.GetString(data).Split(new[] {"\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToArray();

            if (results[0] == "fail" || results[0] == "upgrade")
            {
                this.m_RuntimeServer.Set("view", "main");
                this.m_RuntimeServer.Set("error", "Game not purchased");

                if (results[0] == "upgrade")
                {
                    // Launch the web browser to prompt upgrade.
                    if (results[1].StartsWith("http://") || results[1].StartsWith("https://"))
                    {
                        Process.Start(results[1]);
                    }
                }

                return;
            }

            if (results[0] != "success")
            {
                this.m_RuntimeServer.Set("view", "main");
                this.m_RuntimeServer.Set("error", "Unknown error");
                return;
            }

            this.m_RuntimeServer.Set("working", "License validated");

            ConfigManager.SetSavedConfig(
                resultUsername,
                phid,
                certificate);

            this.m_WorkflowManager.AppendWorkflow(
                this.m_WorkflowFactory.CreateUpdateGameWorkflow());
        }
    }
}

