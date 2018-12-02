using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;
using SpotifyNet.Model.Player;
using SpotifyNet.Model.Player.DeviceData;
using SpotifyNet.Model.Playlists.PlaylistData;
using SpotifyNet.Model.UsersProfile.UserData;

namespace SpotifyNet
{
    public sealed class Spotify : IDisposable
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

        private static readonly HttpClient httpClient;

        private WebAuthorization webAuthorization;
        private AccessToken accessToken;
        private string path;
        private string clientID;
        private string clientSecret;

        static Spotify()
        {
            httpClient = new HttpClient();
        }

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

        private void SetAuthorizationHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AccessToken.access_token}");
        }

        private async Task<T> DownloadDataAsync<T>(string url)
        {
            return await DownloadDataAsync<T>(new Uri(url));
        }

        private async Task<T> DownloadDataAsync<T>(Uri uri)
        {
            SetAuthorizationHeader(httpClient);

            var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
            var responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseMessage);
        }

        public async Task<T> GetNextPageAsync<T, TItem>(T pageing) where T : Pageing<TItem> where TItem : IPageingItem
        {
            if (!pageing.HasNextPage)
                return default;

            return await DownloadDataAsync<T>(pageing.Next);
        }

        public async Task<T> GetPreviousPage<T>(T pageing) where T : Pageing<IPageingItem>
        {
            if (!pageing.HasPreviousPage)
                return default;

            return await DownloadDataAsync<T>(pageing.Next);
        }

        /// <summary>
        /// Get Current User's Profile 
        /// </summary>
        /// <returns></returns>
        public async Task<User> GetMeAsync()
        {
            var url = $"{ api_base_url}/me";

            return await DownloadDataAsync<User>(url);
        }

        /// <summary>
        /// Get a List of Current User's Playlists
        /// </summary>
        /// <param name="limit">The maximum number of playlists to return. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first playlist to return. Maximum offset: 100.000. </param>
        /// <returns></returns>
        public async Task<Playlist> GetPlaylistsAsync(int limit = 20, int offset = 0)
        {
            var uri = new Uri($"{api_base_url}/me/playlists")
                .AddParameter("limit", limit.Clamp(1, 50))
                .AddParameter("offset", offset.Clamp(0, 100_000));

            return await DownloadDataAsync<Playlist>(uri);
        }

        /// <summary>
        /// Get information about a user’s available devices.
        /// </summary>
        /// <returns></returns>
        public async Task<Devices> GetDevicesAsync()
        {
            var url = $"{api_base_url}/me/player/devices";

            return await DownloadDataAsync<Devices>(url);
        }

        #region Player

        /// <summary>
        /// Get information about the user’s current playback state, including track, track progress, and active device.
        /// </summary>
        /// <param name="market">An ISO 3166-1 alpha-2 country code or the string from_token. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public async Task<CurrentPlaybackInfo> GetCurrentPlaybackInfoAsync(string market = "")
        {
            var uri = new Uri($"{api_base_url}/me/player");

            if (!string.IsNullOrEmpty(market))
                uri = uri.AddParameter("market", market);

            return await DownloadDataAsync<CurrentPlaybackInfo>(uri);
        }
        #endregion

        public void Dispose()
        {
            httpClient?.CancelPendingRequests();
            httpClient?.Dispose();
        }
    }
}
