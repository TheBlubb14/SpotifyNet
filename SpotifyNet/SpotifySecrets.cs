namespace SpotifyNet
{
    public struct SpotifySecrets
    {
        public string ClientID { get; set; }

        public string ClientSecret { get; set; }

        public SpotifySecrets(string clientID, string clientSecret)
        {
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
        }
    }
}
