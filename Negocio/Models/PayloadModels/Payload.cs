using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.PayloadModels
{
    public class Payload
    {
        public Payload() { }

        public Merchant Merchant { get; protected set; }
        public string PixKey { get; protected set; }
        public string Description { get; set; }
        public string TxId { get; protected set; }
        public string Amount { get; set; }
        public string Url { get; protected set; }
        public bool UniquePayment { get; protected set; }
    }
}
