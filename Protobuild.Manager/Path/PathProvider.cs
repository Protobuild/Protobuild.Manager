using System;
using System.IO;

namespace Unearth
{
    public class PathProvider : IPathProvider
    {
        public string GetGamePath()
        {
            return Path.Combine(ConfigManager.GetBasePath(), "game");
        }
    }
}

