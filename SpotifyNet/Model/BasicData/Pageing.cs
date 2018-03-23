using Newtonsoft.Json;

namespace SpotifyNet.Model.BasicData
{
    public abstract class Pageing<IPageingItem> : BaseModel
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("items")]
        public IPageingItem[] Items { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        public bool HasNextPage => Next != null;

        public bool HasPreviousPage => Previous != null;
    }
}
