using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Negocio.Requests.RequestModels
{
    public class WebHookRequest
    {
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }
    }
}
