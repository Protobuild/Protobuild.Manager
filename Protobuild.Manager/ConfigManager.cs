namespace Unearth
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class ConfigManager
    {
        public static void SetSavedConfig(
            string username,
            string phid,
            string certificate)
        {
            var userInfoPath = Path.Combine(GetBasePath(), "userinfo");

            using (var writer = new StreamWriter(new FileStream(userInfoPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.WriteLine(username);
                writer.WriteLine(phid);
                writer.WriteLine(string.Empty);
                writer.WriteLine(certificate);
            }
        }

        public static void ClearSavedConfig()
        {
            var userInfoPath = Path.Combine(GetBasePath(), "userinfo");

            if (File.Exists(userInfoPath))
            {
                File.Delete(userInfoPath);
            }
        }

        public static bool GetSavedConfig(
            ref string username,
            ref string phid,
            ref string certificate)
        {
            var userInfoPath = Path.Combine(GetBasePath(), "userinfo");

            if (File.Exists(userInfoPath))
            {
                using (var reader = new StreamReader(new FileStream(userInfoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    username = reader.ReadLine();
                    phid = reader.ReadLine();
                    reader.ReadLine();
                    certificate = reader.ReadLine();

                    return true;
                }
            }

            return false;
        }

        public static string GetBasePath()
        {
            // Look under %appdata%/.tychaia.
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appdata, ".tychaia");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }

        public static string LoadGameOptions() 
        {
            if (!File.Exists(Path.Combine(GetBasePath(), "settings")))
            {
                return "{}";
            }

            using (var reader = new StreamReader(new FileStream(Path.Combine(GetBasePath(), "settings"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var text = reader.ReadToEnd();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return "{}";
                }

                return text;
            }
        }

        public static void SaveGameOptions(string jsonOptions) 
        {
            using (var writer = new StreamWriter(new FileStream(Path.Combine(GetBasePath(), "settings"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.Write(jsonOptions);
            }
        }

        public static string LoadChannel()
        {
            if (File.Exists(Path.Combine(GetBasePath(), "channel")))
            {
                using (var reader = new StreamReader(new FileStream(Path.Combine(GetBasePath(), "channel"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    return reader.ReadToEnd().Trim();
                }
            }

            // Default channel.
            return "stable";
        }

        public static void SaveChannel(string channel)
        {
            using (var writer = new StreamWriter(new FileStream(Path.Combine(GetBasePath(), "channel"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.Write(channel);
            }
        }

#if PLATFORM_WINDOWS
        public static void EnableFullCrashDumps()
        {
            Directory.CreateDirectory(Path.Combine(GetBasePath(), "minidump"));
        }

        public static bool IsFullCrashDumpsEnabled()
        {
            return Directory.Exists(Path.Combine(GetBasePath(), "minidump"));
        }
#endif

        public static List<IPrerequisiteCheck> GetPrerequisiteChecksNotCompleted(List<IPrerequisiteCheck> checks)
        {
            if (!Directory.Exists(Path.Combine(GetBasePath(), "prereq")))
            {
                Directory.CreateDirectory(Path.Combine(GetBasePath(), "prereq"));
            }

            return checks.Where(check => !File.Exists(Path.Combine(GetBasePath(), "prereq", check.ID))).ToList();
        }

        public static void MarkPrerequisiteAsPassed(string id)
        {
            using (var writer = new StreamWriter(Path.Combine(GetBasePath(), "prereq", id)))
            {
                writer.Write("passed");
            }
        }
    }
}
