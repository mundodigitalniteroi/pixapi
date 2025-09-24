using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Models
{
    public class Loc
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("tipoCob")]
        public string TipoCob { get; set; }
    }
}
