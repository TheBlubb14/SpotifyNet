using Newtonsoft.Json;

namespace SpotifyNet.Model.BasicData
{
    public class External_Ids
    {
        [JsonProperty("isrc")]
        public string IsRc { get; set; }
    }
}
