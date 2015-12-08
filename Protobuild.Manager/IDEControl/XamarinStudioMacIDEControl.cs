using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Protobuild.Manager
{
	public class XamarinStudioMacIDEControl : IIDEControl
	{
		public async Task LoadSolution(string modulePath, string moduleName, string targetPlatform, string oldPlatformOnFail, bool isProtobuild)
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = "/usr/bin/open";
			startInfo.Arguments = "-n \"/Applications/Xamarin Studio.app\" --args \"" + Path.Combine(modulePath, moduleName + "." + targetPlatform + ".sln") + "\"";
			startInfo.WorkingDirectory = modulePath;

			var process = new Process();
			process.StartInfo = startInfo;
			process.Start();
			await process.WaitForExitAsync();
		}
	}
}

