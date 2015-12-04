using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Unearth
{
    public class RuntimeServer
    {
        private HttpListener m_ActiveListener;

        private Dictionary<string, object> m_InjectedValues = new Dictionary<string, object>();

        private Action<string> m_RuntimeInjector;

        private object m_InjectorLock = new object();

        private object m_InjectionLock = new object();

        public string BaseUri { get; private set; }

        public void Start()
        {
            var port = 40000;

            var connected = false;
            while (!connected)
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + port + "/");

                try
                {
                    listener.Start();
                    connected = true;
                    this.BaseUri = "http://localhost:" + port + "/";
                    this.m_ActiveListener = listener;
                }
                catch (SocketException)
                {
                    port++;
                }
            }

            var thread = new Thread(this.Run);
            thread.IsBackground = true;
            thread.Start();

            ErrorLog.Log("Started runtime server on " + this.BaseUri);
        }

        public void RegisterRuntimeInjector(Action<string> runtimeInjector)
        {
            this.m_RuntimeInjector = runtimeInjector;

            lock (this.m_InjectionLock)
            {
                this.m_RuntimeInjector(this.GetInjectionScript(false));
            }
        }

        public void Set(string key, object value)
        {
            lock (this.m_InjectorLock)
            {
                this.m_InjectedValues[key] = value;
            }

            if (this.m_RuntimeInjector != null)
            {
                lock (this.m_InjectionLock)
                {
                    try
                    {
                        this.m_RuntimeInjector(this.GetInjectionScript(false, key));
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        this.m_RuntimeInjector(this.GetInjectionScript(false));
                    }
                }
            }
        }

        private string GetInjectionScript(bool firstLoad, string specificKey = null)
        {
            var text = "";
            if (firstLoad)
            {
                text = "window.__state = {};";
            }
            text += "(function() { if (window.__state == undefined || window.__state == null) { return; } ";
            if (specificKey == null)
            {
                text += "window.__state = {};";
            }
            lock (this.m_InjectorLock)
            {
                foreach (var kv in this.m_InjectedValues)
                {
                    if (specificKey != null && kv.Key != specificKey)
                    {
                        continue;
                    }
                    if (kv.Value == null)
                    {
                        text += "window.__state[" + this.EscapeString(kv.Key) + "] = null;";
                    }
                    else if (kv.Value is string)
                    {
                        text += "window.__state[" + this.EscapeString(kv.Key) + "] = " + this.EscapeString((string)kv.Value) + ";";
                    }
                    else if (kv.Value is int)
                    {
                        text += "window.__state[" + this.EscapeString(kv.Key) + "] = " + ((int)kv.Value) + ";";
                    }
                    else if (kv.Value is double)
                    {
                        text += "window.__state[" + this.EscapeString(kv.Key) + "] = " + ((double)kv.Value) + ";";
                    }
                    else if (kv.Value is float)
                    {
                        text += "window.__state[" + this.EscapeString(kv.Key) + "] = " + ((float)kv.Value) + ";";
                    }
                    else if (kv.Value is bool)
                    {
                        if ((bool)kv.Value)
                        {
                            text += "window.__state[" + this.EscapeString(kv.Key) + "] = true;";
                        }
                        else
                        {
                            text += "window.__state[" + this.EscapeString(kv.Key) + "] = false;";
                        }
                    }
                }
            }
            if (firstLoad)
            {
                text += "$(function() { $(document).triggerHandler(\"statechange\", [window.__state]); });";
            }
            else
            {
                text += "$(document).triggerHandler(\"statechange\", [window.__state]);";
            }
            text += " })();";
            return text;
        }

        private string EscapeString(string str)
        {
            return "\"" + str
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\f", "\\f")
                .Replace("\t", "\\t")
                .Replace("/", "\\/")
                .Replace("\"", "\\\"") + "\"";
        }

        private void Run()
        {
            var listener = this.m_ActiveListener;

            var assembly = typeof(RuntimeServer).Assembly;

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                var embeddedUri = request.Url.AbsolutePath.Replace('/', '.');
                if (embeddedUri.EndsWith("."))
                {
                    embeddedUri += "index.htm";
                }

                var resource = assembly.GetManifestResourceStream(embeddedUri.Substring(1));
                if (resource == null)
                {
                    response.StatusCode = 404;
                }
                else
                {
                    var split = embeddedUri.Split('.');
                    var extension = split[split.Length - 1];

                    switch (extension)
                    {
                        case "htm":
                            response.ContentType = "text/html";
                            break;
                        case "js":
                            response.ContentType = "text/javascript";
                            break;
                        case "css":
                            response.ContentType = "text/css";
                            break;
                        case "png":
                            response.ContentType = "image/png";
                            break;
                        case "jpg":
                            response.ContentType = "image/jpeg";
                            break;
                    }

                    if (extension == "htm")
                    {
                        using (var reader = new StreamReader(resource))
                        {
                            var headTpl = this.LoadTemplate(assembly, "tpl.head.htm");

                            var html = reader.ReadToEnd();
                            html = html.Replace("{head}", headTpl + "<script type=\"text/javascript\">" + this.GetInjectionScript(true) + "</script>");

                            var bytes = System.Text.Encoding.UTF8.GetBytes(html);

                            response.ContentLength64 = bytes.Length;
                            response.OutputStream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        response.ContentLength64 = resource.Length;
                        resource.CopyTo(response.OutputStream);
                    }
                }

                response.OutputStream.Close();
            }
        }

        private string LoadTemplate(Assembly assembly, string name)
        {
            var headtpl = assembly.GetManifestResourceStream(name);
            using (var tplReader = new StreamReader(headtpl))
            {
                return tplReader.ReadToEnd();
            }
        }
    }
}

