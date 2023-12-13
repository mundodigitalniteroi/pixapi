using Negocio.Models.PayloadModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Models.CobrancaModels
{
    public class Cobranca
    {
        public Cobranca(string _chave) { this.Chave = _chave; }

        [JsonProperty("chave")]
        public string Chave { get; set; }
        [JsonProperty("solicitacaoPagador")]
        public string SolicitacaoPagador { get; set; }
        [JsonProperty("infoAdicionais")]
        public List<InfoAdicional> InfoAdicionais { get; set; }
        [JsonProperty("valor")]
        public Valor Valor { get; set; }
        [JsonProperty("merchant")]
        public Merchant merchant { get; set; }

        [JsonProperty("qrstring")]
        public string QrTexto { get; set; }

        [JsonProperty("qrCode")]
        public string QrCode { get; set; }

        public Bitmap GerarQRCode(int width, int height, string text)
        {
            try
            {
                var bw = new ZXing.BarcodeWriter();
                var encOptions = new ZXing.Common.EncodingOptions() { Width = width, Height = height, Margin = 0 };
                bw.Options = encOptions;
                bw.Format = ZXing.BarcodeFormat.QR_CODE;
                var resultado = new Bitmap(bw.Write(text));
                return resultado;
            }
            catch
            {
                throw;
            }
        }

    }
}
