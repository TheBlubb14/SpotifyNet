using Newtonsoft.Json;

namespace SpotifyNet.Model.Player.DeviceData
{
    public class Devices : BaseModel
    {
        [JsonProperty("devices")]
        public Device[] Device { get; set; }
    }
}
