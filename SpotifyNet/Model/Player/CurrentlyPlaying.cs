using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;
using SpotifyNet.Model.Tracks;

namespace SpotifyNet.Model.Player
{
    public class CurrentlyPlaying
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("context")]
        public Context Context { get; set; }

        [JsonProperty("progress_ms")]
        public int ProgressMs { get; set; }

        [JsonProperty("item")]
        public Track Item { get; set; }

        [JsonProperty("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; }

        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }
}
