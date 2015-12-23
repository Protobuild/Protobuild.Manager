using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Protobuild.Manager
{
    public class AddinPackageDownload : IAddinPackageDownload
    {
        private readonly IProtobuildProvider _protobuildProvider;
        private readonly IExecution _execution;
        private readonly IProcessLog _processLog;

        public AddinPackageDownload(
            IProtobuildProvider protobuildProvider,
            IExecution execution,
            IProcessLog processLog)
        {
            _protobuildProvider = protobuildProvider;
            _execution = execution;
            _processLog = processLog;
        }

        public async Task<string> GetPackageRoot(string packageUrl)
        {
            var protobuildPath = await _protobuildProvider.GetProtobuild(x => {});

            _processLog.WriteInfo("Installing package from " + packageUrl + "...");
            var process = _execution.ExecuteConsoleExecutable(
                protobuildPath,
                "--install " + packageUrl,
                x =>
                {
                    x.UseShellExecute = false;
                    x.CreateNoWindow = true;
                    x.RedirectStandardOutput = true;
                });
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _processLog.WriteError("Non-zero exit code from Protobuild.exe --install");
                return null;
            }

            var regex = new Regex("^Creating and emptying (?<path>.*)$", RegexOptions.Multiline);
            var match = regex.Match(output);
            if (match.Success)
            {
                _processLog.WriteInfo("Package installation complete.");

                return match.Groups["path"].Value.Trim();
            }

            _processLog.WriteError("Unable to parse result of installing package");
            return null;
        }
    }
}

