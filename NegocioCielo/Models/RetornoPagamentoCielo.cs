using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegocioCielo.Models
{
    public class RetornoCielo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string id_pagamento { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string statusTransacao { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual Transaction transacao { get; set; }
        //public virtual ReturnStatus returnStatus { get; set; }  

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string statusCaptura { get; set; }
                
        public Boolean sucesso { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string tid { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string cod_autorizacao { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string proofOfSales { get; set; }
    }
}
