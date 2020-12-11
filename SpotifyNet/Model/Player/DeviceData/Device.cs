using Newtonsoft.Json;

namespace SpotifyNet.Model.Player.DeviceData
{
    public class Device
    {
        /// <summary>
        /// The device ID. This may be <see langword="null"/>.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// If this device is the currently active device.
        /// </summary>
        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// If this device is currently in a private session.
        /// </summary>
        [JsonProperty("is_private_session")]
        public bool IsPrivateSession { get; set; }

        /// <summary>
        /// Whether controlling this device is restricted. At present if this is <see langword="true"/> then no Web API commands will be accepted by this device.
        /// </summary>
        [JsonProperty("is_restricted")]
        public bool IsRestricted { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Device type, such as “computer”, “smartphone” or “speaker”.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The current volume in percent. This may be <see langword="null"/>.
        /// </summary>
        [JsonProperty("volume_percent")]
        public int VolumePercent { get; set; }
    }
}
