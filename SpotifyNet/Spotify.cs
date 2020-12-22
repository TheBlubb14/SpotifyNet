using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpotifyNet.Model.BasicData;
using SpotifyNet.Model.Player;
using SpotifyNet.Model.Player.DeviceData;
using SpotifyNet.Model.Playlists.PlaylistData;
using SpotifyNet.Model.Tracks;
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
                if (accessToken == null || accessToken.refresh_token == null)
                {
                    var token = LoadAccessToken();

                    // There was no authorization or there was no refresh token provided
                    if (token == default(AccessToken) || token.refresh_token == null)
                    {
                        if (pkceAuthorization is null)
                        {
                            var authorizationCode = webAuthorization.GetAuthorizationCode(clientID, RedirectUri, Scope.All);
                            token = webAuthorization.GetAccessTokenAsync(authorizationCode, RedirectUri, clientID, clientSecret).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var (verifier, challenge) = PKCEUtil.GenerateCodes();
                            var authorizationCode = pkceAuthorization.GetAuthorizationCode(clientID, RedirectUri, Scope.All, challenge);
                            token = pkceAuthorization.GetAccessTokenAsync(authorizationCode, RedirectUri, clientID, verifier).GetAwaiter().GetResult();
                        }
                    }

                    AccessToken = token;
                }

                if (accessToken?.expires_at <= DateTime.Now)
                    AccessToken = webAuthorization.RefreshAccessTokenAsync(accessToken, clientID, clientSecret).GetAwaiter().GetResult() ??
                        pkceAuthorization.RefreshAccessTokenAsync(accessToken, clientID).GetAwaiter().GetResult();

                return accessToken;
            }

            private set
            {
                accessToken = value;
                SaveAccessToken(accessToken);
            }
        }

        private static readonly HttpClient httpClient;

        private readonly WebAuthorization webAuthorization;
        private readonly PKCEAuthorization pkceAuthorization;
        private AccessToken accessToken;
        private readonly string accessTokenPath;
        private readonly string clientID;
        private readonly string clientSecret;

        static Spotify()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// Will use the web authorization to athenticate. Unsecure because client secret will has to be stored.
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        public Spotify(string client_id, string client_secret) : this()
        {
            this.clientID = client_id;
            this.clientSecret = client_secret;
            this.webAuthorization = new WebAuthorization();
        }

        public Spotify(string client_id) : this()
        {
            this.clientID = client_id;
            this.pkceAuthorization = new PKCEAuthorization();
        }

        private Spotify()
        {
            var appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpotifyNet");
            Directory.CreateDirectory(appdata);
            this.accessTokenPath = Path.Combine(appdata, "access_token.json");
        }

        private void SaveAccessToken(AccessToken accessToken)
        {
            File.WriteAllText(accessTokenPath, JsonConvert.SerializeObject(accessToken));
        }

        private AccessToken LoadAccessToken()
        {
            if (!File.Exists(accessTokenPath))
                return default;

            return JsonConvert.DeserializeObject<AccessToken>(File.ReadAllText(accessTokenPath));
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

            using (var response = await httpClient.GetAsync(uri).ConfigureAwait(false))
            {
                var responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(responseMessage);
            }
        }

        private async Task<HttpStatusCode> PutAsync(Uri uri, HttpContent content = null)
        {
            SetAuthorizationHeader(httpClient);

            using (var response = await httpClient.PutAsync(uri, content))
            {
                if (!response.IsSuccessStatusCode)
                    Debug.WriteLine(await response.Content.ReadAsStringAsync());

                return response.StatusCode;
            }
        }

        private async Task<HttpStatusCode> PostAsync(Uri uri, HttpContent content = null)
        {
            SetAuthorizationHeader(httpClient);

            using (var response = await httpClient.PostAsync(uri, content))
            {
                if (!response.IsSuccessStatusCode)
                    Debug.WriteLine(await response.Content.ReadAsStringAsync());

                return response.StatusCode;
            }
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
        /// Get a list of the songs saved in the current Spotify user’s ‘Your Music’ library.
        /// </summary>
        /// <param name="limit">The maximum number of objects to return. Default: 20. Minimum: 1. Maximum: 50.</param>
        /// <param name="offset">The index of the first track to return. Maximum offset: 100.000. </param>
        /// <param name="market">An ISO 3166-1 alpha-2 country code or the string from_token. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public async Task<SavedTracks> GetUsersSavedTracksAsync(int limit = 20, int offset = 0, string market = "")
        {
            var uri = new Uri($"{api_base_url}/me/tracks")
                .AddParameter("limit", limit.Clamp(1, 50))
                .AddParameter("offset", offset.Clamp(0, 100_000));

            return await DownloadDataAsync<SavedTracks>(uri);
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
        /// Get the object currently being played on the user’s Spotify account.
        /// </summary>
        /// <param name="market">An ISO 3166-1 alpha-2 country code or the string from_token. Provide this parameter if you want to apply Track Relinking.</param>
        /// <returns></returns>
        public async Task<CurrentlyPlaying> GetCurrentlyPlayingAsync(string market = "")
        {
            var uri = new Uri($"{api_base_url}/me/player/currently-playing");

            if (!string.IsNullOrEmpty(market))
                uri = uri.AddParameter("market", market);

            return await DownloadDataAsync<CurrentlyPlaying>(uri);
        }

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

        /// <summary>
        /// Pause playback on the user’s account.
        /// </summary>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> PausePlayback(string device_id = "")
        {
            var uri = new Uri($"{api_base_url}/me/player/pause");
            uri = uri.AddParameterString("device_id", device_id);

            return await PutAsync(uri);
        }

        /// <summary>
        /// Skips to previous track in the user’s queue.
        /// </summary>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> SkipPlaybackToPrevious(string device_id = "")
        {
            var uri = new Uri($"{api_base_url}/me/player/previous");

            if (!string.IsNullOrEmpty(device_id))
                uri = uri.AddParameter("device_id", device_id);

            return await PostAsync(uri);
        }

        /// <summary>
        /// Skips to next track in the user’s queue.
        /// </summary>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> SkipPlaybackToNext(string device_id = "")
        {
            var uri = new Uri($"{api_base_url}/me/player/next");

            if (!string.IsNullOrEmpty(device_id))
                uri = uri.AddParameter("device_id", device_id);

            return await PostAsync(uri);
        }

        /// <summary>
        /// Toggle shuffle on or off for user’s playback.
        /// </summary>
        /// <param name="state"><see langword="true"/>: Shuffle user’s playback <see langword="false"/>: Do not shuffle user’s playback.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> ToggleShuffle(bool state, string device_id = null)
        {
            var uri = new Uri($"{api_base_url}/me/player/shuffle")
                .AddParameterString("device_id", device_id)
                .AddParameter("state", state);

            return await PutAsync(uri);
        }

        /// <summary>
        /// Start a new context or resume current playback on the user’s active device.
        /// </summary>
        /// <param name="position_ms">Indicates from what position to start playback. Passing in a position that is greater than the length of the track will cause the player to start playing the next song.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> StartResumePlayback(uint? position_ms = null, string device_id = null)
        {
            return await StartResumePlayback(null, null, null, position_ms, device_id);
        }

        /// <summary>
        /// Start a new context or resume current playback on the user’s active device.
        /// </summary>
        /// <param name="context_uri">Spotify URI of the context to play. Valid contexts are albums, artists, playlists.</param>
        /// <param name="offset">Indicates from where in the context playback should start. Only available when <paramref name="context_uri"/> corresponds to an album or playlist object.</param>
        /// <param name="position_ms">Indicates from what position to start playback. Passing in a position that is greater than the length of the track will cause the player to start playing the next song.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> StartResumePlayback(string context_uri, Offset offset = null, uint? position_ms = null, string device_id = null)
        {
            return await StartResumePlayback(context_uri, null, offset, position_ms, device_id);
        }

        /// <summary>
        /// Start a new context or resume current playback on the user’s active device.
        /// </summary>
        /// <param name="uris">A array of the Spotify track URIs to play.</param>
        /// <param name="offset">Indicates from where in the context playback should start.</param>
        /// <param name="position_ms">Indicates from what position to start playback. Passing in a position that is greater than the length of the track will cause the player to start playing the next song.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> StartResumePlayback(string[] uris, Offset offset = null, uint? position_ms = null, string device_id = null)
        {
            return await StartResumePlayback(null, uris, offset, position_ms, device_id);
        }

        /// <summary>
        /// Start a new context or resume current playback on the user’s active device.
        /// Only one of either <paramref name="context_uri"/> or <paramref name="uris"/> can be specified. If neither is present, calling will resume playback. If both are present the request will return <see cref="HttpStatusCode.BadRequest"/>.
        /// If <paramref name="context_uri"/> is a Playlist or Album, or when <paramref name="uris"/> is provided, then <paramref name="offset"/> can be added to specify starting track in the context.
        /// If the provided <paramref name="context_uri"/> corresponds to an album or playlist object, an <paramref name="offset"/> can be specified either by <see cref="Offset.Uri"/> OR position.If both are present the request will return <see cref="HttpStatusCode.BadRequest"/>. If incorrect values are provided for <see cref="Offset.Position"/> or <see cref="Offset.Uri"/>, the request may be accepted but with an unpredictable resulting action on playback.
        /// </summary>
        /// <param name="context_uri">Spotify URI of the context to play. Valid contexts are albums, artists, playlists.</param>
        /// <param name="uris">A array of the Spotify track URIs to play.</param>
        /// <param name="offset">Indicates from where in the context playback should start. Only available when <paramref name="context_uri"/> corresponds to an album or playlist object, or when the <paramref name="uris"/> parameter is used.</param>
        /// <param name="position_ms">Indicates from what position to start playback. Passing in a position that is greater than the length of the track will cause the player to start playing the next song.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player.
        /// Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        /// If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        private async Task<HttpStatusCode> StartResumePlayback(string context_uri = null, string[] uris = null, Offset offset = null, uint? position_ms = null, string device_id = null)
        {
            var uri = new Uri($"{api_base_url}/me/player/play")
                .AddParameterString("device_id", device_id);

            var body = new
            {
                context_uri,
                uris,
                offset,
                position_ms
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            return await PutAsync(uri, content);
        }

        /// <summary>
        /// Set the volume for the user’s current playback device.
        /// </summary>
        /// <param name="volume_percent">The volume to set. Must be a value from 0 to 100 inclusive.</param>
        /// <param name="device_id">The id of the device this command is targeting. If not supplied, the user’s currently active device is the target.</param>
        /// <returns>
        /// A completed request will return a <see cref="HttpStatusCode.NoContent"/> response code, and then issue the command to the player. Due to the asynchronous nature of the issuance of the command, you should use the <see href="https://developer.spotify.com/documentation/web-api/reference/player/get-information-about-the-users-current-playback">Get Information About The User’s Current Playback</see> endpoint to check that your issued command was handled correctly by the player.
        ///If the device is not found, the request will return <see cref="HttpStatusCode.NotFound"/> response code.
        /// If the user making the request is non-premium, a <see cref="HttpStatusCode.Forbidden"/> response code will be returned.
        /// </returns>
        public async Task<HttpStatusCode> SetPlaybackVolume(int volume_percent, string device_id = null)
        {
            var uri = new Uri($"{api_base_url}/me/player/volume")
                .AddParameter("volume_percent", volume_percent.Clamp(0, 100))
                .AddParameterString("device_id", device_id);

            return await PutAsync(uri);
        }
        #endregion

        public void Dispose()
        {
            httpClient?.CancelPendingRequests();
            httpClient?.Dispose();
        }
    }
}
