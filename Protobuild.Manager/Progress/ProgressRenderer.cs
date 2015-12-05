using System;

namespace Protobuild.Manager
{
    public class ProgressRenderer : IProgressRenderer
    {
        private readonly RuntimeServer m_RuntimeServer;

        public ProgressRenderer(RuntimeServer runtimeServer)
        {
            this.m_RuntimeServer = runtimeServer;
        }

        public void Update(double x, TimeSpan ts)
        {
            var eta = "";
            if (ts.TotalSeconds >= 1)
            {
                eta = "";
                if (ts.TotalHours >= 1)
                {
                    eta += ts.TotalHours.ToString("F0") + " hrs ";
                    eta += ((int)ts.Minutes).ToString("D2") + " mins ";
                    eta += ((int)ts.Seconds).ToString("D2") + " secs ";
                }
                else if (ts.TotalMinutes >= 1)
                {
                    eta += ts.Minutes.ToString("F0") + " mins ";
                    eta += ((int)ts.Seconds).ToString("D2") + " secs ";
                }
                else if (ts.TotalSeconds >= 1)
                {
                    eta += ts.Seconds.ToString("F0") + " secs ";
                }
                eta += "remaining";
            }

            this.m_RuntimeServer.Set("progressAmount", x);
            this.m_RuntimeServer.Set("progressEta", eta);
        }
    }
}

