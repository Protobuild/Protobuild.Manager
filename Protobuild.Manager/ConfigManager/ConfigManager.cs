namespace Protobuild.Manager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class ConfigManager : IConfigManager
    {
        private readonly IBrandingEngine _brandingEngine;

        public ConfigManager(IBrandingEngine brandingEngine)
        {
            _brandingEngine = brandingEngine;
        }
        
        public string GetBasePath()
        {
            // Look under %appdata%/(storage-id).
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, _brandingEngine.ProductStorageID);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
