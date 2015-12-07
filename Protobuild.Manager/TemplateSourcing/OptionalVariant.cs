using System.Collections.Generic;

namespace Protobuild.Manager
{
    public class OptionalVariant
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public List<OptionalVariantOverlay> ProtobuildOptions { get; set; }

        public List<OptionalVariantOverlay> StandardOptions { get; set; }
    }
}