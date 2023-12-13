using Newtonsoft.Json;
using Negocio.Models;
using Negocio.Responses.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negocio.Responses
{
    public class CobConsultaResponse
    {

        [JsonProperty("parametros")]
        public Base.Parametros Parametros { get; set; }

        [JsonProperty("cobs")]
        public List<Cob> Cobs { get; set; }

        [JsonIgnore]
        public int TotalCobsCount => Cobs.Count;

        [JsonIgnore]
        public decimal TotalCobsValor => Cobs.Sum(x => x.Valor.ToDecimal);

        [JsonIgnore]
        public string TotalCobsValorDisplay => TotalCobsValor.ToString("C");
    }
}
