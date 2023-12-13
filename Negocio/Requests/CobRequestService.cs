using Negocio.ApiResource;
using Negocio.Models;
using Negocio.Requests.RequestModels;
using Negocio.Requests.RequestServices.Base;
using Negocio.Responses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Requests.RequestServices
{
    /// <summary>
    /// Reúne endpoints destinados a lidar com gerenciamento de cobranças imediatas.
    /// </summary>
    public class CobRequestService : RequestBase
    {
        public CobRequestService()
        {
            SetRoute("/v1/spi/"+"cob");
        }

        /// <summary>
        /// Criar cobrança imediata
        /// </summary>
        /// <param name="cob"></param>
        /// <returns></returns>
        public async Task<Cob> Create(CobRequest cob)
        {
            return await PostAsync<Cob>(cob);
        }

        /// <summary>
        /// Criar cobrança imediata usando um identificador
        /// </summary>
        /// <param name="txId"></param>
        /// <param name="cob"></param>
        /// <returns></returns>
        public async Task<Cob> Create(string txId, CobRequest cob)
        {
            return await PutAsync<Cob>("/" + txId, cob);
        }

        /// <summary>
        /// Consultar cobrança imediata usando o txId
        /// </summary>
        /// <param name="txId"></param>
        /// <returns></returns>
        public async Task<Cob> GetByTxId(string txId)
        {
            return await GetAsync<Cob>("/" + txId);
        }

        /// <summary>
        /// Consultar lista de cobranças
        /// </summary>
        /// <param name="startDate">A partir de</param>
        /// <param name="endDate">Até (se não informado, por padrão será adicionado 24 horas a partir do startdate)</param>
        /// <returns></returns>
        public async Task<CobConsultaResponse> GetByPeriod(DateTime startDate, DateTime? endDate = null)
        {
            endDate = endDate ?? DateTime.Now;

            return await GetAsync<CobConsultaResponse>("?inicio=" + startDate.ToString("yyyy-MM-ddTHH:mm:ss.000Z") + "&fim=" + endDate.Value.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
        }

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
