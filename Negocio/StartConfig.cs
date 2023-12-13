using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class StartConfig
    {
        /// <summary>
        /// Url da api pix 
        /// </summary>
        public static string BaseUrl { get; private set; }

        /// <summary>
        /// Caminho absoluto do certificado marcado como copy-always
        /// </summary>
        
        /// <summary>
        /// ClientId do oauth 2 do PSP
        /// </summary>
        public static string ClientId { get; private set; }

        /// <summary>
        /// ClientSecret do oauth 2 do PSP
        /// </summary>
        public static string ClientSecret { get; private set; }

        public static X509Certificate2 Certificate { get; private set; }
     
        public StartConfig(string _baseUrl, string _clientId, string _clientSecret, X509Certificate2 _certificate)
        {
            BaseUrl = _baseUrl;
            ClientId = _clientId;
            ClientSecret = _clientSecret;
            Certificate = _certificate;
        }
    }
}