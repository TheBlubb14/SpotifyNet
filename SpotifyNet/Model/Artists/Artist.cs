using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;

namespace SpotifyNet.Model.Artists
{
    public class Artist
    {
        [JsonProperty("external_urls")]
        public External_Urls ExternalUrls { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
