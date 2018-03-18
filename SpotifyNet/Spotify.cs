using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyNet
{
    public class Spotify
    {
        public const string api_base_url = "https://api.spotify.com/v1";


        public Spotify()
        {

        }



        public async void Authorize(string client_id, string client_secret)
        {
            WebAuthorization web = new WebAuthorization();
            string redirectUri = "http://localhost:8000/";
            var authorizationCode = web.GetAuthorizationCode(client_id, redirectUri, Scope.All);
            var accessTokenResult = await web.GetAccessTokenAsync(authorizationCode, redirectUri, client_id, client_secret);
            var d = 1;
            var refreshResult = await web.RefreshAccessTokenAsync(accessTokenResult, client_id, client_secret);
            var aaa = 1;

            //webserver.StartListen(client_id, client_secret);
        }
    }
}
