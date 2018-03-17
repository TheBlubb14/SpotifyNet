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


        public Spotify()
        {

        }



        public void Authorize(string client_id, string client_secret)
        {
            WebAuthorization web = new WebAuthorization();
            var result = web.GetAuthorizationCode(client_id, "http://localhost:8000/", "4711", Scope.All);
            var d = 1;


            //webserver.StartListen(client_id, client_secret);
        }
    }
}
