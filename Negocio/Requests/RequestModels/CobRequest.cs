using Newtonsoft.Json;
using Negocio.Models;
using Negocio.Requests.RequestModels.Base;
using Negocio.Models.PayloadModels;

namespace Negocio.Requests.RequestModels
{
    public class CobRequest : CobBaseRequest
    {
        // Se não for enviado, Json.NET mantém null (default), e o controller trata como Bradesco (0).
        // Use `provider: 1` para Cora.
        [JsonProperty("provider")]
        public string Provider { get; set; } = "bradesco";

        public CobRequest(string _chave) : base(_chave)
        {

        }

        [JsonProperty("calendario")]
        public CalendarioBase Calendario { get; set; }

        [JsonProperty("devedor")]
        public Devedor Devedor { get; set; }
        ///// <summary>
        ///// valores monetários referentes à cobrança.
        ///// </summary>
        //[JsonProperty("valor")]
        //public Valor Valor { get; set; }

        [JsonProperty("parametros")]
        public Parametros Parametros { get; set; }

        [JsonProperty("txId")]
        public string txId { get; set; }

        [JsonProperty("DataInicio")]
        public string DataInicio { get; set; }

        [JsonProperty("DataFim")]
        public string DataFim { get; set; }

        [JsonProperty]
        public string Referencia { get; set; }
    }
}
