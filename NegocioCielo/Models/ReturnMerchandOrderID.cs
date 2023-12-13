using System.Collections.Generic;

namespace NegocioCielo
{
    public class ReturnMerchandOrderID
    {
        public int ReasonCode { get; set; }

        public string ReasonMessage { get; set; }

        public List<PaymentDate> Payments { get; set; }
    }

}
