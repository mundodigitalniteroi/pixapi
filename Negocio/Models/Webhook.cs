﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Models
{
    public class Webhook
    {
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }

        [JsonProperty("chave")]
        public string Chave { get; set; }

        [JsonProperty("criacao")]
        public DateTime Criacao { get; set; }
    }
}
