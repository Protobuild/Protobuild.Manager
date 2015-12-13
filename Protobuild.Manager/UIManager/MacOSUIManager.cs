#if PLATFORM_MACOS
using System;
using AppKit;
using System.IO;

namespace Protobuild.Manager
{
	public class MacOSUIManager : IUIManager
	{
		public static LightweightKernel KernelReference;

		public static NSWindow MainWindow;

		public MacOSUIManager(LightweightKernel kernel) {
			KernelReference = kernel;
		}

		public void Run ()
		{
			NSApplication.Init();
			NSApplication.Main(new string[0]);
		}

		public void Quit ()
		{
			// TODO
		}

		public string SelectExistingProject ()
		{
			var dlg = NSOpenPanel.OpenPanel;
			dlg.CanChooseFiles = false;
			dlg.CanChooseDirectories = true;

			while (true)
			{
				if (dlg.RunModal() == 1)
				{
					var url = dlg.Urls[0];

					if (url != null)
					{
						var path = url.Path;

						var fileInfo = new FileInfo(Path.Combine(path, "Protobuild.exe"));
						if (!fileInfo.Exists || fileInfo.Name.ToLowerInvariant() != "Protobuild.exe".ToLowerInvariant())
						{
							var alert = new NSAlert
							{
								AlertStyle = NSAlertStyle.Critical,
								InformativeText = 
									"It doesn't look like that directory is a Protobuild module.  The directory " +
									"should contain Protobuild.exe to be opened with this tool.",
								MessageText = "Not a Protobuild Module",
							};
							alert.RunModal();
							continue;
						}
						else
						{
							return path;
						}
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
		}

		public bool AskToRepairCorruptProtobuild()
		{
			var alert = new NSAlert
			{
				AlertStyle = NSAlertStyle.Critical,
				MessageText = "Unable to load Protobuild",
				InformativeText =
					"The version of Protobuild.exe in this module could not be loaded " +
					"and may be corrupt.  Do you want to download the latest version " +
					"of Protobuild to repair it, or fallback to the solutions that " +
					"have already been generated?",
			};
			alert.AddButton("Repair (Recommended)");
			alert.AddButton("Use Existing Solutions");
			return alert.RunSheetModal(MainWindow) == 1000;
		}

		public void FailedToRepairCorruptProtobuild()
		{
			var alert = new NSAlert
			{
				AlertStyle = NSAlertStyle.Critical,
				MessageText = "Failed to repair Protobuild",
				InformativeText =
					"This program was unable to repair Protobuild and will now fallback " +
					"to using the existing solutions.",
			};
			alert.AddButton("Okay");
			alert.RunSheetModal(MainWindow);
		}

		public void UnableToLoadModule()
		{
			var alert = new NSAlert
			{
				AlertStyle = NSAlertStyle.Critical,
				MessageText = "Unable to load module",
				InformativeText =
					"This program was unable to load the module or project definition " +
					"information.  Check that the Build/Module.xml and project " +
					"definition files are all valid XML and that they contain no " +
					"errors.  This program will now fallback to using the existing " +
					"solutions.",
			};
			alert.AddButton("Okay");
			alert.RunSheetModal(MainWindow);
		}

		public string BrowseForProjectDirectory ()
		{
			var dlg = NSOpenPanel.OpenPanel;
			dlg.CanChooseFiles = false;
			dlg.CanChooseDirectories = true;

			while (true)
			{
				if (dlg.RunModal() == 1)
				{
					var url = dlg.Urls[0];

					if (url != null)
					{
						var path = url.Path;

						if (new DirectoryInfo(path).GetFiles().Length > 0)
						{
							var alert = new NSAlert
							{
								AlertStyle = NSAlertStyle.Warning,
								InformativeText = 
									"It doesn't look like the selected directory is empty.  You " +
									"should ideally create new projects in empty directories.  " +
									"Use it anyway?",
								MessageText = "Directory Not Empty",
							};
							alert.AddButton("No");
							alert.AddButton("Yes");
							if (alert.RunModal() == 1001)
							{
								return path;
							}
							continue;
						}
						else
						{
							return path;
						}
					}
					else
					{
						return null;
					}
				}
				else
				{
					return null;
				}
			}
		}
	}
}
#endif
