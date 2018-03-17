using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SpotifyNet
{
    internal class WebAuthorization
    {
        const string api_base_url = "https://api.spotify.com/v1";
        const string accounts_base_url = "https://accounts.spotify.com";
        private static readonly HttpClient httpClient;

        static WebAuthorization()
        {
            httpClient = new HttpClient();
        }

        //private string redirectUri;
        //private string clientId;
        //private Scope scope;
        //private string response_type = "token";
        //private string access_token;



        //// Erster Aufruf: /authorize -> client_id, scope, redirect_uri => authorization_code
        //// Zweiter Aufruf: /api/token -> authorization_code, client_secret => access_token, refresh_token
        //// Dritter Aufruf: /api/token -> refresh_token => access_token


        /// <summary>
        /// Erster Aufruf
        /// </summary>
        /// <param name="client_id">Client ID</param>
        /// <param name="redirect_uri">Redirect URL</param>
        /// <param name="state">csrftoken -> Some string to prevent users from Cross-Site-Request-Forgery(CSRF/XSRF)</param>
        /// <param name="scope"></param>
        /// <param name="show_dialog"></param>
        /// <returns>authorization_code</returns>
        public string GetAuthorizationCode(string client_id, string redirect_uri, string state, Scope scope, bool show_dialog = false)
        {
            var uri = new Uri($"{accounts_base_url}/authorize")
                .AddParameter("client_id", client_id)
                .AddParameter("response_type", "code")
                .AddParameter("redirect_uri", redirect_uri.TrimEnd('/'))
                .AddParameter("state", state)
                .AddParameter("scope", string.Join(" ", scope.GetDescriptions()).Trim())
                .AddParameter("show_dialog", show_dialog.ToString().ToLower());

            Process.Start(uri.ToString());
            Uri result;

            using (var server = new Webserver(redirect_uri))
                result = server.WaitListen();

            var values = HttpUtility.ParseQueryString(result.Query);

            if (values["state"] != state)
                throw new Exception($"retrieved state: '{values[state]}' is not '{state}'");

            return values["code"];
        }

        /// <summary>
        /// Zweiter Aufruf
        /// </summary>
        /// <param name="authorization_code"></param>
        /// <param name="client_secret"></param>
        /// <returns>access_token, refresh_token</returns>
        public (string access_token, string refresh_token) ZweiterAufruf(string authorization_code, string client_secret)
        {
            var url = $"{accounts_base_url}/api/token";
            return (null, null);
        }

        /// <summary>
        /// Dritter Aufruf
        /// </summary>
        /// <param name="refresh_token"></param>
        /// <returns>access_token</returns>
        public string DritterAufruf(string refresh_token)
        {
            var url = $"{accounts_base_url}/api/token";
            return null;
        }



        // access_token = token.create_new_token(user.id, type='loadSpotify', data=0)
        // url = SpotifyClient().authorization(client_id, redirect_url, scope=scope, state=access_token)


        //public WebAuthorization(string redirectUri, string clientId, Scope scope)
        //{
        //    this.redirectUri = redirectUri;
        //    this.clientId = clientId;
        //    this.scope = scope;
        //}

        //internal async void GetAccessToken()
        //{
        //    Webserver webserver = new Webserver(redirectUri);
        //    webserver.StartListen();

        //    var a = await authorization_code();
        //    var aa = 1;

        //}

        //private string BuildAuthorizeUri()
        //{
        //    var uri = $"https://accounts.spotify.com/authorize?client_id={this.clientId}&response_type={this.response_type}&redirect_uri={this.redirectUri.TrimEnd('/')}&state=XSS";

        //    if (scope != 0)
        //        uri += "&scope=" + string.Join(" ", scope.GetDescriptions()).Trim();

        //    if (!string.IsNullOrEmpty(access_token))
        //        uri += "&state=" + access_token;

        //    uri += "&show_dialog=false";

        //    return uri;
        //}

        //private async Task<HttpResponseMessage> authorization_code()
        //{
        //    HttpResponseMessage result = null;

        //    var content = new FormUrlEncodedContent(new[]
        //    {
        //        //new KeyValuePair<string, string>("grant_type", grant_type),
        //        new KeyValuePair<string, string>("redirect_uri", redirectUri),
        //        //new KeyValuePair<string, string>("code", code),
        //    });

        //    //using (var client = new HttpClient())
        //    //{

        //    //    result = await client.PostAsync("https://accounts.spotify.com/api/token", content);
        //    //}


        //    //using (var client = new HttpClient())
        //    //{

        //    //    result = await client.PostAsync(BuildAuthorizeUri(), content);
        //    //}

        //    Process.Start(BuildAuthorizeUri());

        //    return result;
        //}
    }
}
