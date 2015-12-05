using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Net;
using System.Xml;
using System.IO;
using System.Linq;

#if FALSE

namespace Protobuild.Manager
{
    internal class NewsLoader : INewsLoader
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IOfflineDetection m_OfflineDetection;

        private readonly IBrandingEngine _brandingEngine;

        internal NewsLoader(RuntimeServer runtimeServer, IOfflineDetection offlineDetection, IBrandingEngine brandingEngine)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_OfflineDetection = offlineDetection;
            _brandingEngine = brandingEngine;
        }

        public void LoadNews()
        {
            Task.Run(Run);
        }

        private async Task Run()
        {
            if (string.IsNullOrWhiteSpace(_brandingEngine.RSSFeedURL))
            {
                return;
            }

            var client = new WebClient();
            var rss = await client.DownloadStringTaskAsync(_brandingEngine.RSSFeedURL);

            var rssXml = new XmlDocument();
            rssXml.LoadXml(rss);

            var items = rssXml.DocumentElement.SelectNodes("channel/item").OfType<XmlElement>();

            var i = 0;
            foreach (var item in items)
            {
                this.m_RuntimeServer.Set("newsContent" + i, item.SelectNodes("description").OfType<XmlElement>().First().InnerText);
                this.m_RuntimeServer.Set("newsAuthor" + i, item.SelectNodes("creator").OfType<XmlElement>().First().InnerText);
                this.m_RuntimeServer.Set("newsDate" + i, item.SelectNodes("pubDate").OfType<XmlElement>().First().InnerText);
                this.m_RuntimeServer.Set("newsTitle" + i, item.SelectNodes("title").OfType<XmlElement>().First().InnerText);
                i++;
            }
            this.m_RuntimeServer.Set("newsCount", i);
        }
    }
}

#endif