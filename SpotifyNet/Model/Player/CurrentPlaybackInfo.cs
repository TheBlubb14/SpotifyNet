using Newtonsoft.Json;
using SpotifyNet.Model.Player.DeviceData;
using SpotifyNet.Model.Tracks;

namespace SpotifyNet.Model.Player
{
    public class CurrentPlaybackInfo
    {
        [JsonProperty("device")]
        public Device Device { get; set; }

        [JsonProperty("shuffle_state")]
        public bool ShuffleState { get; set; }

        [JsonProperty("repeat_state")]
        public string RepeatState { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("context")]
        public object Context { get; set; }

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
