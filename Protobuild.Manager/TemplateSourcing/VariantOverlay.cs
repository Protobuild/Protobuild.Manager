namespace Protobuild.Manager
{
    public class VariantOverlay
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string OverlayPath { get; set; }

        public string[] EnableServices { get; set; }

        public string[] DisableServices { get; set; }
    }
}