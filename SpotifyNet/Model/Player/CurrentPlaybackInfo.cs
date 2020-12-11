using Newtonsoft.Json;
using SpotifyNet.Model.Player.DeviceData;
using SpotifyNet.Model.Tracks;

namespace SpotifyNet.Model.Player
{
    /// <summary>
    /// Currently Playing Context
    /// </summary>
    public class CurrentPlaybackInfo
    {
        /// <summary>
        /// The device that is currently active
        /// </summary>
        [JsonProperty("device")]
        public Device Device { get; set; }

        /// <summary>
        /// If shuffle is on or off
        /// </summary>
        [JsonProperty("shuffle_state")]
        public bool ShuffleState { get; set; }

        /// <summary>
        /// off, track, context
        /// </summary>
        [JsonProperty("repeat_state")]
        public string RepeatState { get; set; }

        /// <summary>
        /// Unix Millisecond Timestamp when data was fetched
        /// </summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// A Context Object. Can be null (e.g. If private session is enabled this will be null).
        /// </summary>
        [JsonProperty("context")]
        public object Context { get; set; }

        /// <summary>
        /// Progress into the currently playing track. Can be null (e.g. If private session is enabled this will be null).
        /// </summary>
        [JsonProperty("progress_ms")]
        public int? ProgressMs { get; set; }

        /// <summary>
        /// The currently playing track or episode. Can be null (e.g. If private session is enabled this will be null).
        /// </summary>
        [JsonProperty("item")]
        public Track Item { get; set; }

        /// <summary>
        /// The object type of the currently playing item. Can be one of track, episode, ad or unknown.
        /// </summary>
        [JsonProperty("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; }

        /// <summary>
        /// Allows to update the user interface based on which playback actions are available within the current context
        /// </summary>
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }
}
