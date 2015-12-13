using System;

namespace Protobuild.Manager
{
    public interface IUIManager
    {
        void Run();

        void Quit();

        string SelectExistingProject();

		bool AskToRepairCorruptProtobuild();

		void FailedToRepairCorruptProtobuild();

		void UnableToLoadModule();

        string BrowseForProjectDirectory();
    }
}

