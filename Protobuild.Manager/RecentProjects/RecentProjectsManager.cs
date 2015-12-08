using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class RecentProjectsManager : IRecentProjectsManager, ILoadable
    {
        private readonly RuntimeServer _runtimeServer;

        private readonly IConfigManager _configManager;

        public RecentProjectsManager(
            RuntimeServer runtimeServer, 
            IConfigManager configManager)
        {
            _runtimeServer = runtimeServer;
            _configManager = configManager;
        }

        public async Task Load()
        {
            var recentItemsFile = Path.Combine(_configManager.GetBasePath(), "recent-projects.txt");
            var recentItems = new List<KeyValuePair<string, string>>();

            if (File.Exists(recentItemsFile))
            {
                using (var stream = new StreamReader(recentItemsFile))
                {
                    while (!stream.EndOfStream)
                    {
                        var title = stream.ReadLine();
                        var path = stream.ReadLine();
                        recentItems.Add(new KeyValuePair<string, string>(title, path));
                    }
                }
            }

            recentItems.Reverse();

            var i = 0;
            foreach (var kv in recentItems)
            {
                _runtimeServer.Set("recentProjectTitle" + i, kv.Key);
                _runtimeServer.Set("recentProjectPath" + i, kv.Value);
                i++;
            }

            _runtimeServer.Set("recentProjectsCount", i);
			_runtimeServer.Flush();
        }

        public void AddEntry(string name, string path)
        {
            var recentItemsFile = Path.Combine(_configManager.GetBasePath(), "recent-projects.txt");
            var recentItems = new List<KeyValuePair<string, string>>();

            if (File.Exists(recentItemsFile))
            {
                using (var stream = new StreamReader(recentItemsFile))
                {
                    while (!stream.EndOfStream)
                    {
                        var title = stream.ReadLine();
                        var existingPath = stream.ReadLine();
                        if (existingPath == path)
                        {
                            continue;
                        }
                        recentItems.Add(new KeyValuePair<string, string>(title, existingPath));
                    }
                }
            }

            recentItems.Add(new KeyValuePair<string, string>(name, path));

            using (var stream = new StreamWriter(recentItemsFile))
            {
                foreach (var kv in recentItems)
                {
                    stream.WriteLine(kv.Key);
                    stream.WriteLine(kv.Value);
                }
            }
        }
    }
}

