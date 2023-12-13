using Negocio.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Requests.RequestServices
{
    public static class TokenService
    {
        private static Token LastToken { get; set; }

  

        public static Token Create()
        {
            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            /*if (!string.IsNullOrEmpty(LastToken?.AccessToken))
                return LastToken;*/

            var byteArray = new UTF8Encoding().GetBytes(StartConfig.ClientId + ":" + StartConfig.ClientSecret);

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(StartConfig.Certificate);
            handler.PreAuthenticate = true;
            
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol =  SecurityProtocolType.Tls12;
            //ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));            

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, StartConfig.BaseUrl + "/oauth/token"))
            {
                var content = "grant_type=client_credentials";

                requestMessage.Content = new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded");

                var s = client.SendAsync(requestMessage).Result;

                var data = s.Content.ReadAsStringAsync().Result;

                if (s.IsSuccessStatusCode)
                    LastToken = JsonConvert.DeserializeObject<Token>(data, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                else
                    throw new ArgumentException(data);

                return LastToken;
            }
        }

    }
}
