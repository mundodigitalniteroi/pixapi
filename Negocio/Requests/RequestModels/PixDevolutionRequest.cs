using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Requests.RequestModels
{
    public class PixDevolutionRequest
    {
        [JsonProperty("valor")]
        public string Valor { get; set; }
    }
}
