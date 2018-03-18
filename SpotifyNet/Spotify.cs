using Newtonsoft.Json;
using System;
using System.IO;

namespace SpotifyNet
{
    public sealed class Spotify
    {
        public const string api_base_url = "https://api.spotify.com/v1";
        public const string RedirectUri = "http://localhost:8000/";

        internal AccessToken AccessToken
        {
            get
            {
                if (accessToken == null)
                {
                    var token = LoadAccessToken();

                    // There was no authorization
                    if (token == default(AccessToken))
                    {
                        var authorizationCode = webAuthorization.GetAuthorizationCode(clientID, RedirectUri, Scope.All);
                        token = webAuthorization.GetAccessTokenAsync(authorizationCode, RedirectUri, clientID, clientSecret).Result;
                    }

                    AccessToken = token;
                }

                if (accessToken?.expires_at <= DateTime.Now)
                    AccessToken = webAuthorization.RefreshAccessTokenAsync(accessToken, clientID, clientSecret).Result;

                return accessToken;
            }

            private set
            {
                accessToken = value;
                SaveAccessToken(accessToken);
            }
        }

        private WebAuthorization webAuthorization;
        private AccessToken accessToken;
        private string path;
        private string clientID;
        private string clientSecret;

        public Spotify(string client_id, string client_secret)
        {
            clientID = client_id;
            clientSecret = client_secret;
            webAuthorization = new WebAuthorization();

            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpotifyNet");
            Directory.CreateDirectory(path);
            path = Path.Combine(path, "access_token.json");
        }



        private void SaveAccessToken(AccessToken accessToken)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(accessToken));
        }

        private AccessToken LoadAccessToken()
        {
            if (!File.Exists(path))
                return default;

            return JsonConvert.DeserializeObject<AccessToken>(File.ReadAllText(path));
        }

        public void test()
        {
            var a = AccessToken;
        }
    }
}
