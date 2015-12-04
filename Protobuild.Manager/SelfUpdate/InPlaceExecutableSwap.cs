using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace Unearth
{
    public class InPlaceExecutableSwap
    {
        private readonly IExecution m_Execution;

        public InPlaceExecutableSwap(IExecution execution)
        {
            this.m_Execution = execution;
        }

        public void SwapWith(string targetPath)
        {
            var executable = this.CompileSwapExecutable(targetPath);

#if !PLATFORM_WINDOWS
            try
            {
                var p = Process.Start("chmod", "u+x '" + executable + "'");
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception("Can't mark game as executable");
            }
#endif

            this.m_Execution.ExecuteConsoleExecutable(executable);
        }

        private string CompileSwapExecutable(string src)
        {
            var srcEscaped = src.Replace("\"", "\"\"");
            var destEscaped = Assembly.GetExecutingAssembly().Location.Replace("\"", "\"\"");

#if PLATFORM_WINDOWS
            var launchCode = @"
startInfo.FileName = dest;
startInfo.WorkingDirectory = new FileInfo(dest).Directory.FullName;
";
#elif PLATFORM_LINUX
            var launchCode = @"
try
{
    var p = Process.Start(""chmod"", ""u+x '"" + dest + ""'"");
    p.WaitForExit();

    startInfo.FileName = dest;
    startInfo.WorkingDirectory = new FileInfo(dest).Directory.FullName;
}
catch (Exception)
{
    Console.WriteLine(""Unable to mark as executable, going to try using /usr/bin/mono explicitly."");

    startInfo.FileName = ""/usr/bin/mono"";
    startInfo.Arguments = dest;
    startInfo.WorkingDirectory = new FileInfo(dest).Directory.FullName;
}
";
#else
#error Not Implemented
#endif

            var code = @"
using System;
using System.Diagnostics;
using System.IO;

public static class Program
{
    public static void Main(string[] args)
    {
        var src = @""" + srcEscaped + @""";
        var dest = @""" + destEscaped + @""";
        
        Console.WriteLine(""Swapping "" + dest + "" with "" + src + ""."");

        while (true)
        {
            try
            {
                Console.WriteLine(""Attempting copy."");
                System.IO.File.Copy(src, dest, true);
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(""Copy failed.  Retrying soon..."");
                Console.WriteLine(ex);
                System.Threading.Thread.Sleep(100);
            }
        }

        Console.WriteLine(""Relaunching launcher..."");
        var startInfo = new ProcessStartInfo();
" + launchCode + @"

        Process.Start(startInfo);
    }
}
";

            var codeProvider = new CSharpCodeProvider();
            var icc = codeProvider.CreateCompiler();
            var output = Path.Combine(Path.GetTempPath(), "SwapUnearthLauncher.exe");

            var parameters = new CompilerParameters();
            parameters.GenerateExecutable = true;
            parameters.OutputAssembly = output;
            parameters.ReferencedAssemblies.Add(typeof(ProcessStartInfo).Assembly.Location);
            CompilerResults results = icc.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.Count > 0)
            {
                throw new InvalidOperationException("Unable to build swap executable");
            }

            return output;
        }
    }
}

