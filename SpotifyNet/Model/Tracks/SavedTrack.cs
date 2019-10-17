using Newtonsoft.Json;
using System;

namespace SpotifyNet.Model.Tracks
{
    public class SavedTrack : ISavedTrack
    {
        [JsonProperty("added_at")]
        public DateTime AddedAt { get; set; }

        [JsonProperty("track")]
        public Track Track { get; set; }
    }
}
