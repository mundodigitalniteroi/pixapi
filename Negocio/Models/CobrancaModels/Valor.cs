using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.CobrancaModels
{
    public class Valor
    {
      
        [JsonProperty("original")]
        public string Original { get; set; }
        [JsonIgnore]
        public decimal ToDecimal { get; }
        [JsonIgnore]
        public string Display { get; }
    }
}
