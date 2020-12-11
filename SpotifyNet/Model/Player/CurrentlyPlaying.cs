using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;
using SpotifyNet.Model.Tracks;

namespace SpotifyNet.Model.Player
{
    /// <summary>
    /// Currently Playing Object
    /// </summary>
    public class CurrentlyPlaying
    {
        /// <summary>
        /// Unix Millisecond Timestamp when data was fetched
        /// </summary>
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        /// <summary>
        /// A Context Object. Can be null.
        /// </summary>
        [JsonProperty("context")]
        public Context Context { get; set; }

        /// <summary>
        /// Progress into the currently playing track or episode. Can be null.
        /// </summary>
        [JsonProperty("progress_ms")]
        public int? ProgressMs { get; set; }

        /// <summary>
        /// The currently playing track or episode. Can be null.
        /// </summary>
        [JsonProperty("item")]
        public Track Item { get; set; }

        /// <summary>
        /// The object type of the currently playing item. Can be one of track, episode, ad or unknown.
        /// </summary>
        [JsonProperty("currently_playing_type")]
        public string CurrentlyPlayingType { get; set; }

        /// <summary>
        /// If something is currently playing.
        /// </summary>
        [JsonProperty("is_playing")]
        public bool IsPlaying { get; set; }
    }
}
