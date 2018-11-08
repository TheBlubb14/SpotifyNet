using Newtonsoft.Json;

namespace SpotifyNet.Model.Playlists.PlaylistData
{
    public class Tracks
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
