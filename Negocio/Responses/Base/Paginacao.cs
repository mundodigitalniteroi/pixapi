using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Responses.Base
{
    public class Paginacao
    {
        [JsonProperty("paginaAtual")]
        public int PaginaAtual { get; set; }

        [JsonProperty("itensPorPagina")]
        public int ItensPorPagina { get; set; }

        [JsonProperty("quantidadeDePaginas")]
        public int QuantidadeDePaginas { get; set; }

        [JsonProperty("quantidadeTotalDeItens")]
        public int QuantidadeTotalDeItens { get; set; }
    }
}
