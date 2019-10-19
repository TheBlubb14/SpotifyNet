using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace SpotifyNet
{
    internal class WebAuthorization
    {
        public const string accounts_base_url = "https://accounts.spotify.com";

        private static readonly HttpClient httpClient;

        static WebAuthorization()
        {
            httpClient = new HttpClient();
        }

        /// <summary>
        /// 1. GetAuthorizationCode
        /// </summary>
        /// <param name="client_id">Client ID</param>
        /// <param name="redirect_uri">Redirect URL</param>
        /// <param name="scope"></param>
        /// <param name="show_dialog"></param>
        /// <returns>authorization_code</returns>
        public string GetAuthorizationCode(string client_id, string redirect_uri, Scope scope, bool show_dialog = false)
        {
            // Some string to prevent users from Cross-Site-Request-Forgery(CSRF/XSRF)
            string csrftoken = Guid.NewGuid().ToString();

            var uri = new Uri($"{accounts_base_url}/authorize")
                .AddParameter("client_id", client_id)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", redirect_uri.TrimEnd('/'))
                .AddParameter("state", csrftoken)
                .AddParameter("scope", string.Join(" ", scope.GetDescriptions()).Trim())
                .AddParameter("show_dialog", show_dialog.ToString().ToLower());

            Uri result;

            using (var server = new Webserver(redirect_uri))
            {
                Process.Start(new ProcessStartInfo(uri.ToString())
                {
                    // Explicitly set UseShellExecute to true, 
                    // because in .NetFramework the default value is true but in
                    // .NetCore the default value changed to false
                    // See: https://github.com/dotnet/winforms/issues/1520#issuecomment-515899341
                    // and https://github.com/dotnet/corefx/issues/24704
                    UseShellExecute = true
                });
                result = server.WaitListen();
            }

            var values = HttpUtility.ParseQueryString(result.Query);

            if (values["state"] != csrftoken)
                throw new Exception($"retrieved state: '{values[csrftoken]}' is not '{csrftoken}'");

            return values["code"];
        }

        /// <summary>
        /// 2. GetAccessToken
        /// </summary>
        /// <param name="authorization_code">Authorization Code from <see cref="GetAuthorizationCode"/></param>
        /// <param name="redirect_uri">The same Redirect URI which was used in <see cref="GetAuthorizationCode"/></param>
        /// <param name="client_id">Client ID</param>
        /// <param name="client_secret">Client Secret</param>
        /// <returns><see cref="AccessToken"/></returns>

        public async Task<AccessToken> GetAccessTokenAsync(string authorization_code, string redirect_uri, string client_id, string client_secret)
        {
            var url = $"{accounts_base_url}/api/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorization_code),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri.Trim('/'))
            });

            string responseMessage = "";

            SetAuthorizationHeader(httpClient, client_id, client_secret);
            using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                    responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var result = JsonConvert.DeserializeObject<AccessToken>(responseMessage);
                CalculateExpiration(ref result);

                return result;
            }
        }

        /// <summary>
        /// 3. RefreshAccessToken
        /// </summary>
        /// <param name="accessToken">Access Token</param>
        /// <param name="client_id">Client ID</param>
        /// <param name="client_secret">Client Secret</param>
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> RefreshAccessTokenAsync(AccessToken accessToken, string client_id, string client_secret)
        {
            var url = $"{accounts_base_url}/api/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", accessToken.refresh_token),
            });

            SetAuthorizationHeader(httpClient, client_id, client_secret);

            using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    accessToken = JsonConvert.DeserializeObject<AccessToken>(responseMessage);

                    CalculateExpiration(ref accessToken);
                }
                else
                {
                    throw new Exception();
                }

                return accessToken;
            }
        }

        private void CalculateExpiration(ref AccessToken accessToken)
        {
            accessToken.expires_at = DateTime.Now.AddSeconds(accessToken.expires_in);
        }


        private void SetAuthorizationHeader(HttpClient httpClient, string client_id, string client_secret)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{client_id}:{client_secret}")));
        }
    }
}
