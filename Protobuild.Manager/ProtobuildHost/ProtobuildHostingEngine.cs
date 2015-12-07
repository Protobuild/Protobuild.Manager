using System;
using System.IO;
using System.Reflection;

namespace Protobuild.Manager
{
    public class ProtobuildHostingEngine : IProtobuildHostingEngine
    {
        public ModuleHost LoadModule(string modulePath)
        {
            var path = Path.Combine(modulePath, "Protobuild.exe");

            var assembly = Assembly.LoadFrom(path);
            var stream = assembly.GetManifestResourceStream("Protobuild.Internal.dll.lzma");

            /*
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Stream stream = null;
            Assembly assembly = null;
            foreach (var a in assemblies)
            {
                if (a.IsDynamic)
                {
                    continue;
                }

                var s = a.GetManifestResourceStream("Protobuild.Internal.dll.lzma");
                if (s != null)
                {
                    stream = s;
                    assembly = a;
                    break;
                }
            }
            */

            if (stream == null)
            {
                throw new InvalidOperationException("Compressed LZMA version of Protobuild.Internal not found!");
            }

            var memory = new MemoryStream();
            var lzmaHelperType = assembly.GetType("LZMA.LzmaHelper", true);
            var lzmaDecompressMethod = lzmaHelperType.GetMethod("Decompress");
            lzmaDecompressMethod.Invoke(null, new object[] {stream, memory, null});

            var bytes = new byte[memory.Position];
            memory.Seek(0, SeekOrigin.Begin);
            memory.Read(bytes, 0, bytes.Length);
            memory.Close();

            var internalAssembly = Assembly.Load(bytes);

            var moduleInfoType = internalAssembly.GetType("Protobuild.ModuleInfo");
            var moduleInfoLoad = moduleInfoType.GetMethod("Load");
            return new ModuleHost
            {
                LoadedModule = moduleInfoLoad.Invoke(null,
                    new object[] {Path.Combine(modulePath, "Build", "Module.xml")})
            };
        }
    }
}