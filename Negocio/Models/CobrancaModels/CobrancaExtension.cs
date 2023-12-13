using Negocio.Models.PayloadModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.CobrancaModels
{
    public static class CobrancaExtention
    {        
        public static Payload ToPayload(this Cobranca cobranca, string txId, Merchant merchant) => (Payload)new StaticPayload(cobranca.Chave, txId, merchant, cobranca?.Valor?.Original, cobranca?.SolicitacaoPagador);
        
    }

    public class StaticPayload : Payload
    {
        public StaticPayload(
          string _pixKey,
          string _txId,
          Merchant _merchant,
          string _amount = null,
          string _description = "")
        {
            this.PixKey = _pixKey;
            this.Description = _description;
            this.Merchant = _merchant;
            this.TxId = _txId;
            this.Amount = _amount;
        }
    }
}
