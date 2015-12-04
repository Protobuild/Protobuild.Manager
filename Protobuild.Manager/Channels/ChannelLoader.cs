using System;
using System.Threading;
using Phabricator.Conduit;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace Unearth
{
    public class ChannelLoader : IChannelLoader
    {
        private readonly RuntimeServer m_RuntimeServer;

        public ChannelLoader(RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
        }

        public void LoadInBackground()
        {
            var thread = new Thread(
                () =>
            {
                var client = new ConduitClient(UrlConfig.CONDUIT);
                client.User = "channel-query";
                client.Certificate = "tmml6tmiq3tf5jiy6666r5eow6zazpct3r7f5lngtj7brq"
                    + "26tdxwkqvttjqthunbh7kaxwgqm5vpai4a64q6ildb3q3b"
                    + "h52xfb4palp7urymrfldqcvbdenlpajfvtv6n5ohgulhir"
                    + "vbf53ltdmajintv6z76e3pn725prs5ok2nwuvhqtefnvr6"
                    + "eg37iurlb4z47mhjudjroq3kkica6pojqeacerjzda3tzx"
                    + "5kehlz63c2vycf7uo3imcvogt";

                var currentChannel = ConfigManager.LoadChannel();

                ArrayList result;
                try
                {
                    result = client.Do<ArrayList>(
                        "phragment.querysnapshots",
                        new
                    {
                        path = this.GetPlatformArchive()
                    });
                }
                catch (WebException ex)
                {
                    // Can't update snapshot list (we might be offline).
                    this.PushSnapshotsList(new List<string> { currentChannel });
                    ErrorLog.Log(ex);
                    return;
                }
                catch (Exception ex)
                {
                    // Can't update snapshot list (time might be wrong).
                    this.PushSnapshotsList(new List<string> { currentChannel });
                    ErrorLog.Log(ex);
                    return;
                }

                var snapshots = new List<string>();
                foreach (var snapshot in result)
                {
                    snapshots.Add((string)(((Dictionary<string, object>)snapshot)["name"]));
                }

                this.PushSnapshotsList(snapshots.OrderBy(x => x).ToList());
            });
            thread.IsBackground = true;
            thread.Start();
        }

        private void PushSnapshotsList(List<string> snapshots)
        {
            for (var i = 0; i < snapshots.Count; i++)
            {
                this.m_RuntimeServer.Set(
                    "availableChannel" + i.ToString(System.Globalization.NumberFormatInfo.InvariantInfo),
                    snapshots[i]);
            }

            this.m_RuntimeServer.Set("availableChannelCount", snapshots.Count.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        private string GetPlatformArchive()
        {
#if PLATFORM_LINUX
            return "Unearth-Linux.zip";
#elif PLATFORM_WINDOWS
            return "Unearth-Windows.zip";
#elif PLATFORM_MACOS
            return "Unearth-MacOS.zip";
#else
#error Not Supported
#endif
        }
    }
}

