using Newtonsoft.Json;
using Negocio.Models;
using Negocio.Responses.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negocio.Responses
{
    public class PixConsultaResponse
    {
        [JsonProperty("parametros")]
        public Base.Parametros Parametros { get; set; }

        [JsonProperty("pix")]
        public List<Pix> Pix { get; set; }

        [JsonIgnore]
        public int TotalPixCount => Pix.Count;

        [JsonIgnore]
        public decimal TotalPixValor => Pix.Sum(x => x.ValorToDecimal);

        [JsonIgnore]
        public string TotalPixValorDisplay => TotalPixValor.ToString("C");
    }
}
