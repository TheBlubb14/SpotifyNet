using Newtonsoft.Json;

namespace SpotifyNet.Model.BasicData
{
    public class External_Urls
    {
        [JsonProperty("spotify")]
        public string Spotify { get; set; }
    }
}
