using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.PayloadModels
{
    public class DynamicPayload : Payload
    {
        public DynamicPayload(string _txId, Merchant _merchant, string _url, bool _uniquePayment = false, string _amount = null, string _description = "") 
        {
            this.TxId = _txId;
            this.Merchant = _merchant;
            this.Url = _url;
            this.UniquePayment = _uniquePayment;
            this.Amount = _amount;
            this.Description = _description;
        }
    }
}
