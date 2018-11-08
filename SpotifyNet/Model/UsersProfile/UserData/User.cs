using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;

namespace SpotifyNet.Model.UsersProfile.UserData
{
    public class User : BaseModel
    {
        [JsonProperty("birthdate")]
        public string Birthdate { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("display_name")]
        public object DisplayName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("external_urls")]
        public External_Urls ExternalUrls { get; set; }

        [JsonProperty("followers")]
        public Followers Followers { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("images")]
        public object[] Images { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
