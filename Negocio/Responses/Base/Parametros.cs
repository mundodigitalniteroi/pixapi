using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Responses.Base
{
    public class Parametros
    {
        [JsonProperty("inicio")]
        public DateTime Inicio { get; set; }

        [JsonProperty("fim")]
        public DateTime Fim { get; set; }

        [JsonProperty("paginacao")]
        public Paginacao Paginacao { get; set; }
    }
}
