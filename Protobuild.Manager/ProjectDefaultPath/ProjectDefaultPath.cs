using System;
using System.Collections.Generic;
using System.IO;

namespace Protobuild.Manager
{
    public class ProjectDefaultPath : IProjectDefaultPath
    {
        private readonly IConfigManager _configManager;

        public ProjectDefaultPath(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public string GetProjectDefaultPath()
        {
            var lastProjectPathFile = Path.Combine(_configManager.GetBasePath(), "last-project-path.txt");

            if (File.Exists(lastProjectPathFile))
            {
                using (var stream = new StreamReader(lastProjectPathFile))
                {
                    return stream.ReadToEnd().Trim();
                }
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar;
        }

        public void SetProjectDefaultPath(string path)
        {
            var lastProjectPathFile = Path.Combine(_configManager.GetBasePath(), "last-project-path.txt");

            using (var stream = new StreamWriter(lastProjectPathFile))
            {
                stream.Write(path);
            }
        }
    }
}