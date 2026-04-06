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
        public string Certificate { get; set; }
        public string SenhaCertificado { get; set;}
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
