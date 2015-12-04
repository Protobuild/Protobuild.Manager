using System;
using Phabricator.Conduit;
using System.Threading;
using System.Collections.Generic;
using System.Collections;

namespace Unearth
{
    using System.Net;

    public class NewsLoader : INewsLoader
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IOfflineDetection m_OfflineDetection;

        public NewsLoader(RuntimeServer runtimeServer, IOfflineDetection offlineDetection)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_OfflineDetection = offlineDetection;
        }

        public void LoadNews()
        {
            var thread = new Thread(this.Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Run()
        {
            var client = new ConduitClient(UrlConfig.CONDUIT);

            ArrayList posts;
            try
            {
                posts = client.Do<ArrayList>("unearth.getlauncherposts", new { });
            }
            catch (WebException)
            {
                this.m_OfflineDetection.MarkAsOffline();
                return;
            }

            var i = 0;
            foreach (var postDict in posts)
            {
                var post = (Dictionary<string, object>)postDict;
                var content = (string)post["content"];
                var author = (string)post["author"];
                var date = (string)post["date"];
                var title = (string)post["title"];
                this.m_RuntimeServer.Set("newsContent" + i, content);
                this.m_RuntimeServer.Set("newsAuthor" + i, author);
                this.m_RuntimeServer.Set("newsDate" + i, date);
                this.m_RuntimeServer.Set("newsTitle" + i, title);
                i++;
            }

            this.m_RuntimeServer.Set("newsCount", i);
        }
    }
}

