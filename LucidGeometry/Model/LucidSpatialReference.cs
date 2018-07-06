using Newtonsoft.Json;

namespace LucidGeometry
{
    public class LucidSpatialReference
    {
        [JsonProperty("wkid")]
        public int WKID { get; set; }

        [JsonProperty("wellknownId")]
        public int WellknownId { get; set; }
    }
}
