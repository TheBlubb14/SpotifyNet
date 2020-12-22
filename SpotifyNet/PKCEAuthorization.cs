using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SpotifyNet
{
    /// <summary>
    /// The authorization code flow with PKCE is the best option for mobile and desktop applications
    /// where it is unsafe to store your client secret. It provides your app with an access token
    /// that can be refreshed. For further information about this flow, see
    /// <see href="https://tools.ietf.org/html/rfc7636">IETF RFC-7636</see>.
    /// </summary>
    /// <remarks>https://developer.spotify.com/documentation/general/guides/authorization-guide/#authorization-code-flow-with-proof-key-for-code-exchange-pkce</remarks>
    internal class PKCEAuthorization
    {
        public const string accounts_base_url = "https://accounts.spotify.com";

        private static readonly HttpClient httpClient;

        static PKCEAuthorization()
        {
            httpClient = new HttpClient();
        }

        public PKCEAuthorization()
        {
            // TODO: testen und danach mit WebAuthorization vereinheitlichen, vielleicht sogar mit interface?
            // oder die methoden mergen und mit parameter overlaoding
        }

        public string GetAuthorizationCode(string client_id, string redirect_uri, Scope scope, string code_challenge)
        {
            // Some string to prevent users from Cross-Site-Request-Forgery(CSRF/XSRF)
            string csrftoken = Guid.NewGuid().ToString();

            var uri = new Uri($"{accounts_base_url}/authorize")
                .AddParameter("client_id", client_id)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", redirect_uri.TrimEnd('/'))
                .AddParameter("code_challenge_method", "S256")
                .AddParameter("code_challenge", code_challenge)
                .AddParameter("state", csrftoken)
                .AddParameter("scope", string.Join(" ", scope.GetDescriptions()).Trim());

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
        /// Your app exchanges the code for an access token
        /// </summary>
        /// <param name="authorization_code">The authorization code obtained in step <see cref="RedirectUserToAuthorization"/></param>
        /// <param name="redirect_uri">The value of this parameter must match the value of the redirect_uri parameter your app supplied when requesting the authorization code.</param>
        /// <param name="client_id">The client ID for your app, available from the developer dashboard.</param>
        /// <param name="code_verifier">The value of this parameter must match the value of the code_verifier that your app generated in <see cref="CreateCodeVerifierAndChallenge"/></param>
        /// <returns></returns>
        public async Task<AccessToken> GetAccessTokenAsync(string authorization_code, string redirect_uri, string client_id, string code_verifier)
        {
            var url = $"{accounts_base_url}/api/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", client_id),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorization_code),
                new KeyValuePair<string, string>("redirect_uri", redirect_uri.Trim('/')),
                new KeyValuePair<string, string>("code_verifier", code_verifier),
            });

            string responseMessage = "";

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
        /// <returns><see cref="AccessToken"/></returns>
        public async Task<AccessToken> RefreshAccessTokenAsync(AccessToken accessToken, string client_id)
        {
            var url = $"{accounts_base_url}/api/token";

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", accessToken.refresh_token),
                new KeyValuePair<string, string>("client_id", client_id),
            });

            using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
            {
                var responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    accessToken = JsonConvert.DeserializeObject<AccessToken>(responseMessage);

                    CalculateExpiration(ref accessToken);
                }
                else
                {
                    throw new Exception(responseMessage);
                }

                return accessToken;
            }
        }

        private void CalculateExpiration(ref AccessToken accessToken)
        {
            accessToken.expires_at = DateTime.Now.AddSeconds(accessToken.expires_in);
        }
    }
}
