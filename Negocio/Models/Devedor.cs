﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Models
{
    public class Devedor
    {
        [JsonProperty("cnpj")]
        public string Cnpj { get; set; }

        [JsonProperty("cpf")]
        public string Cpf { get; set; }

        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonIgnore]
        public bool IsCNPJ => !string.IsNullOrEmpty(Cnpj);

        [JsonIgnore]
        public bool IsCPF => !string.IsNullOrEmpty(Cpf);

        [JsonIgnore]
        public string DocumentInfo => IsCNPJ ? Cnpj : Cpf;
    }
}
