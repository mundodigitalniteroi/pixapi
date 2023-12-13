using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models
{
    public class Parametros
    {
        public string BaseUrl { get; set; }

        /// <summary>
        /// Caminho absoluto do certificado marcado como copy-always
        /// </summary>

        /// <summary>
        /// ClientId do oauth 2 do PSP
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// ClientSecret do oauth 2 do PSP
        /// </summary>
        public  string ClientSecret { get; set; }

        public String Certificate { get; set; }

        public string SenhaCertificado { get; set;}

    }
}
