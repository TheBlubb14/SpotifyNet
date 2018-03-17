using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace SpotifyNet
{
    internal class Webserver : IDisposable
    {
        //public const string ListenOn = "http://localhost:8000/";

        private HttpListener _httpListener;

        public Webserver(string listenOn)
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(listenOn);
            _httpListener.Start();
        }

        public void Dispose()
        {
            _httpListener?.Stop();
            _httpListener = null;
        }

        public Uri WaitListen()
        {
            try
            {
                var context = _httpListener.GetContext();
                var response = context.Response;
                response.AddHeader("Content-Type", "text/html");
                var content = Encoding.UTF8.GetBytes(
                    "<html>" +
                    "<head>" +
                        "<title>SpotifyNet</title>" +
                    "</head>" +
                    "<body>" +
                        "<p> You can now close this window </p>" +
                    "</body>" +
                    "</html>");
                response.OutputStream.Write(content, 0, content.Length);
                response.OutputStream.Close();

                return context.Request.Url;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                return null;
            }
        }

        public async void StartListen()
        {
            try
            {
                var context = await _httpListener.GetContextAsync();
                var response = context.Response;
                response.AddHeader("Content-Type", "text/html");
                var content = Encoding.UTF8.GetBytes(
                    "<html>" +
                    "<head>" +
                        "<title>SpotifyNet</title>" +
                    "</head>" +
                    "<body>" +
                        "<p> You can now close this window </p>" +
                    "</body>" +
                    "</html>");
                await response.OutputStream.WriteAsync(content, 0, content.Length);
                response.OutputStream.Close();

                var request = context.Request;

                using (var reader = new StreamReader(request.InputStream))
                {
                    var result = await reader.ReadToEndAsync();
                    var sss = result.Base64Decode(response.ContentEncoding);
                    var dd = 1;
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
            }
        }
    }
}
