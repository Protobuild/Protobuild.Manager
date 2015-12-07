namespace Protobuild.Manager
{
    public interface IProjectCreator
    {
        void CreateProject(CreateProjectRequest request);
    }
}