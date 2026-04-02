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
        /// Base URL do authorization server (mTLS) para a Cora (Integração Direta).
        /// Se não informado, será usada a URL Stage padrão.
        /// </summary>
        public string CoraTokenBaseUrl { get; set; }
        /// <summary>
        /// Base URL da API de invoices (QR Code Pix) para a Cora (Integração Direta).
        /// Se não informado, será usada a URL Stage padrão.
        /// </summary>
        public string CoraApiBaseUrl { get; set; }

        /// <summary>
        /// ClientId do oauth 2 do PSP
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// ClientSecret do oauth 2 do PSP
        /// </summary>
        public  string ClientSecret { get; set; }
        /// <summary>
        /// Caminho do arquivo de certificado (relativo à raiz do projeto ou absoluto)
        /// </summary>
        public string CertificateFileName { get; set; }
        /// <summary>
        /// Conteúdo do certificado em base64 (mantido para compatibilidade)
        /// </summary>
        public string Certificate { get; set; }
        public string SenhaCertificado { get; set;}
    }
}
