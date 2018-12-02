using Newtonsoft.Json;

namespace SpotifyNet.Model.BasicData
{
    public class Image
    {
        /// <summary>
        /// The image height in pixels.
        /// </summary>
        [JsonProperty("height")]
        public int? Height { get; set; }

        /// <summary>
        /// The source URL of the image.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// The image width in pixels.
        /// </summary>
        [JsonProperty("width")]
        public int? Width { get; set; }
    }
}
