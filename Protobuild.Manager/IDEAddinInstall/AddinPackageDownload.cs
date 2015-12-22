using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Protobuild.Manager
{
    public class AddinPackageDownload : IAddinPackageDownload
    {
        private readonly IProtobuildProvider _protobuildProvider;
        private readonly IExecution _execution;

        public AddinPackageDownload(
            IProtobuildProvider protobuildProvider,
            IExecution execution)
        {
            _protobuildProvider = protobuildProvider;
            _execution = execution;
        }

        public async Task<string> GetPackageRoot(string packageUrl)
        {
            var protobuildPath = await _protobuildProvider.GetProtobuild(x => {});

            var process = _execution.ExecuteConsoleExecutable(
                protobuildPath,
                "--install " + packageUrl,
                x =>
                {
                    x.UseShellExecute = false;
                    x.RedirectStandardOutput = true;
                });
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("Non-zero exit code from Protobuild.exe --install");
            }

            var regex = new Regex("^Creating and emptying (?<path>.*)$", RegexOptions.Multiline);
            var match = regex.Match(output);
            if (match.Success)
            {
                return match.Groups["path"].Value;
            }

            throw new InvalidOperationException("Unable to parse result of installing package");
        }
    }
}

