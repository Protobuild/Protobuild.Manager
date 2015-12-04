using System;
using System.IO;
using System.Net;

namespace Unearth
{
    public static class ErrorLog
    {
        private static object m_LogLock = new object();

        public static void Log(Exception ex)
        {
            lock (m_LogLock)
            {
                var logPath = Path.Combine(ConfigManager.GetBasePath(), "launcher.log");

                using (var writer = new StreamWriter(logPath, true))
                {
                    var webException = ex as WebException;

                    if (webException == null)
                    {
                        writer.WriteLine("=== Non-Web Exception! ===");
                        writer.WriteLine("Exception:");
                        writer.WriteLine(ex);
                    }
                    else
                    {
                        writer.WriteLine("=== Web Exception! ===");
                        writer.WriteLine("Exception:");
                        writer.WriteLine(ex);
                        writer.WriteLine("Status: " + webException.Status);

                        if (webException.Response != null)
                        {
                            writer.WriteLine("Headers: ");
                            writer.WriteLine(webException.Response.Headers);
                            writer.WriteLine("Response URI: " + webException.Response.ResponseUri);
                            writer.WriteLine("Content Type: " + webException.Response.ContentType);
                            writer.WriteLine("Content Length: " + webException.Response.ContentLength);

                            var stream = webException.Response.GetResponseStream();

                            writer.WriteLine("Response: ");
                            if (stream != null)
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    writer.WriteLine(reader.ReadToEnd());
                                }
                            }
                            else
                            {
                                writer.WriteLine("(not present)");
                            }
                        }
                        else
                        {
                            writer.WriteLine("Response: ");
                            writer.WriteLine("(no response from server)");
                        }
                    }
                }
            }
        }

        public static void Log(string s)
        {
            lock (m_LogLock)
            {
                var logPath = Path.Combine(ConfigManager.GetBasePath(), "launcher.log");

                using (var writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine(s);
                }
            }
        }
    }
}

