#if PLATFORM_LINUX || PLATFORM_MACOS
namespace Protobuild.Manager
{
    public class Mono380Check : AbstractPrerequisiteCheck
    {
        public override string ID
        {
            get
            {
                return "mono-380";
            }
        }

        public override string Name
        {
            get
            {
                return "Mono 3.8.0 or greater";
            }
        }

        public override void Check()
        {
            this.Status = PrerequisiteCheckStatus.Passed;
        }
    }
}
#endif