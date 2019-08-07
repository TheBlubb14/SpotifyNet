using Newtonsoft.Json;

namespace SpotifyNet.Model.Player
{
    /// <summary>
    /// Indicates from where in the context playback should start.
    /// </summary>
    public class Offset
    {
        /// <summary>
        /// The zero based position for the track in the playlist or album.
        /// </summary>
        [JsonProperty("position")]
        public uint? Position { get; private set; }

        /// <summary>
        /// Representing the track uri.
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; private set; }

        /// <summary>
        /// Indicates from where in the context playback should start.
        /// </summary>
        /// <param name="position">Representing the uri of the item to start at.</param>
        public Offset(uint position)
        {
            Position = position;
        }

        /// <summary>
        /// Indicates from where in the context playback should start.
        /// </summary>
        /// <param name="uri">The zero based position for the track in the playlist or album.</param>
        public Offset(string uri)
        {
            Uri = uri;
        }
    }
}
