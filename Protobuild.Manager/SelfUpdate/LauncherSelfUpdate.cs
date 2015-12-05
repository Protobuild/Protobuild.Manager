using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;

namespace Protobuild.Manager
{
    public class LauncherSelfUpdate : ILauncherSelfUpdate
    {
        private readonly RuntimeServer m_RuntimeServer;

        private readonly IProgressRenderer m_ProgressRenderer;

        private readonly IExecution m_Execution;

        private readonly IUIManager m_UIManager;

        public LauncherSelfUpdate(
            RuntimeServer runtimeServer,
            IProgressRenderer progressRenderer,
            IExecution execution,
            IUIManager uiManager)
        {
            this.m_RuntimeServer = runtimeServer;
            this.m_ProgressRenderer = progressRenderer;
            this.m_Execution = execution;
            this.m_UIManager = uiManager;
        }

        public void StartCheck()
        {
            /*if (File.Exists(Path.Combine(ConfigManager.GetBasePath(), "disable-launcher-update")))
            {
                return;
            }*/

            var thread = new Thread(this.Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Run()
        {
            /*
            var client = new ConduitClient(UrlConfig.CONDUIT);
            client.User = "channel-query";
            client.Certificate = "tmml6tmiq3tf5jiy6666r5eow6zazpct3r7f5lngtj7brq"
                + "26tdxwkqvttjqthunbh7kaxwgqm5vpai4a64q6ildb3q3b"
                + "h52xfb4palp7urymrfldqcvbdenlpajfvtv6n5ohgulhir"
                + "vbf53ltdmajintv6z76e3pn725prs5ok2nwuvhqtefnvr6"
                + "eg37iurlb4z47mhjudjroq3kkica6pojqeacerjzda3tzx"
                + "5kehlz63c2vycf7uo3imcvogt";

#if PLATFORM_LINUX
            var name = "UnearthLauncher/Linux/Unearth.exe";
#elif PLATFORM_MACOS
            var name = "UnearthLauncher/MacOS/Unearth.zip/Unearth.app/Contents/MonoBundle/Unearth.exe";
#elif PLATFORM_WINDOWS
            var name = "UnearthLauncher/Windows/Unearth.exe";
#endif

            var result = client.Do<Dictionary<string, object>>("phragment.queryfragments", new
            {
                paths = new[] { name },
                snapshot = "stable"
            });

            var entries = (ArrayList)result[name];

            if (entries.Count == 0)
            {
                return;
            }

            var dict = (Dictionary<string, object>)entries[0];
            var hash = (string)dict["hash"];

            if (hash == this.HashOfThisAssembly())
            {
                return;
            }

            this.m_RuntimeServer.Set("view", "launching");
            this.m_RuntimeServer.Set("welcome", "Updating game launcher");

            var toDownload = new Dictionary<string, string>
            {
                { "Unearth.exe", (string)dict["uri"] }
            };

            long totalBytes = int.Parse((string)dict["size"], CultureInfo.InvariantCulture.NumberFormat);

            var downloader = new ProgressRetryDownloader();
            var downloadResult = downloader.DownloadFiles(
                toDownload,
                ConfigManager.GetBasePath(),
                this.OnStatus,
                this.OnUpdateProgress,
                this.OnFail,
                totalBytes);

            if (!downloadResult)
            {
                return;
            }

            this.m_RuntimeServer.Set("progressAmount", null);
            this.m_RuntimeServer.Set("progressEta", null);
            this.m_RuntimeServer.Set("working", "");

            var swap = new InPlaceExecutableSwap(this.m_Execution);
            swap.SwapWith(Path.Combine(ConfigManager.GetBasePath(), "Unearth.exe"));

            this.m_UIManager.Quit();
            */
        }

        private string HashOfThisAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var sha1 = new SHA1Cng();

            var stream = new FileStream(assembly.Location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var hashBytes = sha1.ComputeHash(stream);
            stream.Dispose();

            return BitConverter.ToString(hashBytes)
                .Replace("-", string.Empty);
        }

        private void OnStatus(string status)
        {
            this.m_RuntimeServer.Set("working", status);
        }

        private void OnFail(string error)
        {
            this.m_RuntimeServer.Set("progressAmount", null);
            this.m_RuntimeServer.Set("progressEta", null);
            this.m_RuntimeServer.Set("view", "main");
            this.m_RuntimeServer.Set("error", error);
        }

        private void OnUpdateProgress(float progress, TimeSpan eta)
        {
            this.m_ProgressRenderer.Update(progress, eta);
        }
    }
}

