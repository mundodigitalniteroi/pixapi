using Negocio;
using Negocio.Models;
using Negocio.Models.CobrancaModels;
using Negocio.Models.PayloadModels;
using Negocio.Requests.RequestModels;
using Negocio.Requests.RequestServices;
using Negocio.Responses;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeraPixMundoDigital.Controllers
{
    [RoutePrefix("api/pix")]
    public class PixController : ApiController
    {

        [AcceptVerbs("POST")]
        [Route("GerarPix")]
        public async Task<Cob> GerarCobrancaPix(CobRequest _cobranca)
        {
            if (_cobranca.Parametros != null)
            {

                byte[] ArquivoCertificado = Convert.FromBase64String(_cobranca.Parametros.Certificate);

                new StartConfig(
                    _baseUrl: _cobranca.Parametros.BaseUrl,
                    _clientId: _cobranca.Parametros.ClientId,
                    _clientSecret: _cobranca.Parametros.ClientSecret,
                    _certificate: new System.Security.Cryptography.X509Certificates.X509Certificate2(ArquivoCertificado, _cobranca.Parametros.SenhaCertificado));

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.MaxAllowed);
                    store.Add(StartConfig.Certificate);
                    store.Close();
                }

                ServicePointManager.Expect100Continue = true;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12|SecurityProtocolType.Tls|SecurityProtocolType.Tls11| SecurityProtocolType.Ssl3;
            }

            var cobRequest = new CobRequestService();

            var cb = await cobRequest.Create(System.Guid.NewGuid().ToString("N"), _cobranca);

            var payload = cb.ToPayload(new Merchant(_cobranca.merchant.Name, _cobranca.merchant.City));

            var stringToQrCode = payload.GenerateStringToQrCode();

            cb.QrTexto = stringToQrCode;

            using (var ms = new MemoryStream())
            {
                using (var bitmap = new Bitmap(cobRequest.GerarQRCode(200, 200, stringToQrCode)))
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    cb.QrCode = Convert.ToBase64String(ms.GetBuffer()); //Get Base64
                }
            }
            

            return cb;
        }


        [AcceptVerbs("POST")]
        [Route("GerarPixEstatico")]
        public async Task<Cobranca> GerarPixEstatico(Cobranca cobranca)
        {  
            
            var now = DateTime.Now;
            var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
            int uniqueId = (int)(zeroDate.Ticks / 10000);

            var payload = cobranca.ToPayload(uniqueId.ToString(), cobranca.merchant);

            var stringToQrCode = payload.GenerateStringToQrCode();

            cobranca.QrTexto = stringToQrCode;

            using (var ms = new MemoryStream())
            {
                using (var bitmap = new Bitmap(cobranca.GerarQRCode(200, 200, stringToQrCode)))
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    cobranca.QrCode = Convert.ToBase64String(ms.GetBuffer()); //Get Base64
                }
            }

            return cobranca;
        }

        [AcceptVerbs("POST")]
        [Route("ConsultarPix")]
        public async Task<Cob> ConsultarPix(CobRequest _cobranca)
        {
            if (_cobranca.Parametros != null)
            {

                byte[] ArquivoCertificado = Convert.FromBase64String(_cobranca.Parametros.Certificate);

                new StartConfig(
                    _baseUrl: _cobranca.Parametros.BaseUrl,
                    _clientId: _cobranca.Parametros.ClientId,
                    _clientSecret: _cobranca.Parametros.ClientSecret,
                    _certificate: new System.Security.Cryptography.X509Certificates.X509Certificate2(ArquivoCertificado, _cobranca.Parametros.SenhaCertificado));

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(StartConfig.Certificate);
                    store.Close();
                }
            }

            var cobRequest = new CobRequestService();

            var cb = await cobRequest.GetByTxId(_cobranca.txId);

            return cb;
        }

        [AcceptVerbs("POST")]
        [Route("ConsultarPixPeriodo")]
        public async Task<CobConsultaResponse> CobGetByPeriod(CobRequest _cobranca)
        {

            if (_cobranca.Parametros != null)
            {

                byte[] ArquivoCertificado = Convert.FromBase64String(_cobranca.Parametros.Certificate);

                new StartConfig(
                    _baseUrl: _cobranca.Parametros.BaseUrl,
                    _clientId: _cobranca.Parametros.ClientId,
                    _clientSecret: _cobranca.Parametros.ClientSecret,
                    _certificate: new System.Security.Cryptography.X509Certificates.X509Certificate2(ArquivoCertificado, _cobranca.Parametros.SenhaCertificado));

                using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(StartConfig.Certificate);
                    store.Close();
                }
            }

            var cobRequest = new CobRequestService();


            DateTime? date_fim = null;

            if (!string.IsNullOrEmpty(_cobranca.DataFim))
                date_fim = Convert.ToDateTime(_cobranca.DataFim);
            
            var cb = await cobRequest.GetByPeriod(Convert.ToDateTime(_cobranca.DataInicio), date_fim);

            return cb;
        }
    }
}
