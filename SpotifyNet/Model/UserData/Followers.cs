using Newtonsoft.Json;

namespace SpotifyNet.Model.UserData
{
    public class Followers
    {
        /// <summary>
        /// A link to the Web API endpoint providing full details of the followers.
        /// Please note that this will always be set to null, as the Web API does not support it at the moment.
        /// </summary>
        [JsonProperty("href")]
        public object Href { get; set; }

        /// <summary>
        /// The total number of followers.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
