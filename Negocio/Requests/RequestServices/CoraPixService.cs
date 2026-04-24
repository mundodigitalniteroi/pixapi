using Negocio.Config;
using Negocio.Extentions;
using Negocio.Models;
using Negocio.Models.CobrancaModels;
using Negocio.Requests.RequestModels;
using Negocio.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;

namespace Negocio.Requests.RequestServices
{
    public class CoraPixService
    {
        public async Task<Cob> Create(string txId, CobRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var idempotencyKey = Guid.NewGuid().ToString("D");

            var clientId = request.Parametros.ClientId;
            var certPath = request.Parametros.Certificate;
            var certPassword = request.Parametros.SenhaCertificado;
            var apiBaseUrl = request.Parametros.BaseUrl;


            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("ClientId não configurado para integração com a Cora.");
            if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(certPath))
                throw new ArgumentException("Certificado/key não configurados para integração com a Cora.");

            byte[] ArquivoCertificado = Convert.FromBase64String(certPath);

            X509Certificate2 certificate = new X509Certificate2(ArquivoCertificado, certPassword);

            var accessToken = await CreateAccessToken(apiBaseUrl, clientId, certificate).ConfigureAwait(false);

            var payloadA = BuildPayload(txId, request);

            var invoiceResult = await CreateInvoice(apiBaseUrl, accessToken, idempotencyKey, payloadA, certificate).ConfigureAwait(false);

            if (!invoiceResult.IsSuccessStatusCode)
            {
                throw new ArgumentException(invoiceResult.ResponseBody ?? "Erro ao criar cobrança Pix na Cora.");
            }

            var invoice = DeserializeInvoice(invoiceResult.ResponseBody);

            var status = !string.IsNullOrWhiteSpace(invoice.Status) ? MapCoraStatusToCobStatus(invoice.Status) : "CREATED";
            var emv = invoice.Pix != null ? invoice.Pix.Emv : null;

            var cob = new Cob(request.Chave)
            {
                Txid = invoice.Id ?? txId,
                Status = status,
                QrTexto = emv,
                QrCode = !string.IsNullOrWhiteSpace(emv) ? GenerateQrCodeBase64(emv) : null,

                Valor = request.Valor,
                SolicitacaoPagador = request.SolicitacaoPagador
            };

            return cob;
        }

        public async Task<Cob> GetByReferencia(CobRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var invoiceId = request.txId;
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentException("txId (id da invoice da Cora) não informado.");

            var clientId = request.Parametros.ClientId;
            var certPath = request.Parametros.Certificate;
            var certPassword = request.Parametros.SenhaCertificado;
            var apiBaseUrl = request.Parametros.BaseUrl;

            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("ClientId não configurado para integração com a Cora.");
            if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(certPath))
                throw new ArgumentException("Certificado/key não configurados para integração com a Cora.");

            byte[] ArquivoCertificado = Convert.FromBase64String(certPath);
            X509Certificate2 certificate = new X509Certificate2(ArquivoCertificado, certPassword);

            var accessToken = await CreateAccessToken(apiBaseUrl, clientId, certificate).ConfigureAwait(false);

            var invoiceResult = await GetInvoice(apiBaseUrl, accessToken, invoiceId, certificate).ConfigureAwait(false);
            if (!invoiceResult.IsSuccessStatusCode)
                throw new ArgumentException(invoiceResult.ResponseBody ?? "Erro ao consultar cobrança Pix na Cora.");

            var invoice = DeserializeInvoice(invoiceResult.ResponseBody);

            var status = !string.IsNullOrWhiteSpace(invoice.Status) ? MapCoraStatusToCobStatus(invoice.Status) : "CREATED";
            var emv = invoice.Pix != null ? invoice.Pix.Emv : null;

            var cob = new Cob(request.Chave)
            {
                Txid = invoice.Id ?? invoiceId,
                Status = status,
                QrTexto = emv,
                QrCode = !string.IsNullOrWhiteSpace(emv) ? GenerateQrCodeBase64(emv) : null,

                Valor = request.Valor ?? BuildValorFromTotalAmountCents(invoice.TotalAmount),
                SolicitacaoPagador = request.SolicitacaoPagador,
                Devedor = request.Devedor ?? BuildDevedorFromCustomer(invoice.Customer != null ? invoice.Customer.Name : null, invoice.Customer != null && invoice.Customer.Document != null ? invoice.Customer.Document.Type : null, invoice.Customer != null && invoice.Customer.Document != null ? invoice.Customer.Document.Identity : null)
            };

            return cob;
        }

        public async Task<CobConsultaResponse> GetByPeriod(CobRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var clientId = request.Parametros.ClientId;
            var certPath = request.Parametros.Certificate;
            var certPassword = request.Parametros.SenhaCertificado;
            var apiBaseUrl = request.Parametros.BaseUrl;

            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("ClientId não configurado para integração com a Cora.");
            if (string.IsNullOrWhiteSpace(certPath) || string.IsNullOrWhiteSpace(certPath))
                throw new ArgumentException("Certificado/key não configurados para integração com a Cora.");
            if (string.IsNullOrWhiteSpace(request.DataInicio))
                throw new ArgumentException("DataInicio não informado.");

            DateTime startDate;
            if (!DateTime.TryParse(request.DataInicio, out startDate))
                throw new ArgumentException("DataInicio inválido.");

            DateTime? endDate = null;
            if (!string.IsNullOrWhiteSpace(request.DataFim))
            {
                DateTime parsedEnd;
                if (!DateTime.TryParse(request.DataFim, out parsedEnd))
                    throw new ArgumentException("DataFim inválido.");
                endDate = parsedEnd;
            }

            byte[] ArquivoCertificado = Convert.FromBase64String(certPath);
            X509Certificate2 certificate = new X509Certificate2(ArquivoCertificado, certPassword);

            var accessToken = await CreateAccessToken(apiBaseUrl, clientId, certificate).ConfigureAwait(false);

            var invoicesResult = await GetInvoices(apiBaseUrl, accessToken, startDate, endDate, certificate).ConfigureAwait(false);
            if (!invoicesResult.IsSuccessStatusCode)
                throw new ArgumentException(invoicesResult.ResponseBody ?? "Erro ao consultar cobranças na Cora.");

            var cobs = new List<Cob>();

            var invoices = DeserializeInvoiceList(invoicesResult.ResponseBody);
            foreach (var invoice in invoices)
            {
                var status = !string.IsNullOrWhiteSpace(invoice.Status) ? MapCoraStatusToCobStatus(invoice.Status) : "CREATED";
                var emv = invoice.Pix != null ? invoice.Pix.Emv : null;

                cobs.Add(new Cob(request.Chave)
                {
                    Txid = invoice.Id,
                    Status = status,
                    QrTexto = emv,
                    QrCode = !string.IsNullOrWhiteSpace(emv) ? GenerateQrCodeBase64(emv) : null,
                    Valor = BuildValorFromTotalAmountCents(invoice.TotalAmount),
                    Devedor = BuildDevedorFromCustomer(invoice.Customer != null ? invoice.Customer.Name : null, invoice.Customer != null && invoice.Customer.Document != null ? invoice.Customer.Document.Type : null, invoice.Customer != null && invoice.Customer.Document != null ? invoice.Customer.Document.Identity : null)
                });
            }

            var response = new CobConsultaResponse
            {
                Parametros = new Negocio.Responses.Base.Parametros
                {
                    Inicio = startDate,
                    Fim = endDate ?? DateTime.Now
                },
                Cobs = cobs
            };

            return response;
        }

        private async Task<string> CreateAccessToken(string tokenBaseUrl, string clientId, X509Certificate2 certificate)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(certificate);
            handler.PreAuthenticate = true;

            using (var client = new HttpClient(handler))
            {
                var tokenUrl = tokenBaseUrl.TrimEnd('/') + "/token";

                var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
            });

                var response = await client.PostAsync(tokenUrl, formData);

                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Body: {body}");

                var token = JsonConvert.DeserializeObject<Token>(body);
                return token != null ? token.AccessToken : null;
            }
        }

        private async Task<InvoiceCreateResult> GetInvoices(string apiBaseUrl, string accessToken, DateTime startDate, DateTime? endDate, X509Certificate2 certificate)
        {
            var url = apiBaseUrl.TrimEnd('/') + "/v2/invoices";

            var query = new List<string>
            {
                "start_date=" + Uri.EscapeDataString(startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            };

            if (endDate != null)
                query.Add("end_date=" + Uri.EscapeDataString(endDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

            url = url + "?" + string.Join("&", query);

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ClientCertificates.Add(certificate);
                handler.PreAuthenticate = true;

                using (var client = new HttpClient(handler))
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return new InvoiceCreateResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        StatusCode = response.StatusCode,
                        ResponseBody = responseBody
                    };
                }
            }
        }

        private async Task<InvoiceCreateResult> CreateInvoice(string apiBaseUrl, string accessToken, string idempotencyKey, object payload, X509Certificate2 certificate)
        {
            var invoiceUrl = apiBaseUrl.TrimEnd('/') + "/v2/invoices//";

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ClientCertificates.Add(certificate);
                handler.PreAuthenticate = true;

                using (var client = new HttpClient(handler))
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, invoiceUrl))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    requestMessage.Headers.Add("Idempotency-Key", idempotencyKey);
                    requestMessage.Headers.Add("x-idempotency-id", idempotencyKey);
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var json = JsonConvert.SerializeObject(payload);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return new InvoiceCreateResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        StatusCode = response.StatusCode,
                        ResponseBody = responseBody
                    };
                }
            }
        }

        private async Task<InvoiceCreateResult> GetInvoice(string apiBaseUrl, string accessToken, string invoiceId, X509Certificate2 certificate)
        {
            var invoiceUrl = apiBaseUrl.TrimEnd('/') + "/v2/invoices/" + invoiceId;

            using (var handler = new HttpClientHandler())
            {
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ClientCertificates.Add(certificate);
                handler.PreAuthenticate = true;

                using (var client = new HttpClient(handler))
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, invoiceUrl))
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.SendAsync(requestMessage).ConfigureAwait(false);
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return new InvoiceCreateResult
                    {
                        IsSuccessStatusCode = response.IsSuccessStatusCode,
                        StatusCode = response.StatusCode,
                        ResponseBody = responseBody
                    };
                }
            }
        }

        private object BuildPayload(string txId, CobRequest request)
        {
            var amountCents = 500;
            if (!string.IsNullOrWhiteSpace(request?.Valor?.Original))
            {
                var decimalValue = request.Valor.Original.ToDecimalUSCulture();
                amountCents = (int)Math.Round(decimalValue * 100m, MidpointRounding.AwayFromZero);
                if (amountCents < 500)
                    amountCents = 500;
            }

            var customerName = !string.IsNullOrWhiteSpace(request?.Devedor?.Nome) ? request.Devedor.Nome : "Cliente Pix";

            var identity = request?.Devedor != null
                ? (request.Devedor.IsCNPJ ? request.Devedor.Cnpj : request.Devedor.Cpf)
                : null;

            if (!string.IsNullOrWhiteSpace(identity))
                identity = new string(identity.Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(identity))
                identity = "00000000000";

            var documentType = request?.Devedor != null && request.Devedor.IsCNPJ ? "CNPJ" : "CPF";
            if (string.IsNullOrWhiteSpace(documentType))
                documentType = identity.Length > 11 ? "CNPJ" : "CPF";

            var description = !string.IsNullOrWhiteSpace(request?.SolicitacaoPagador)
                ? request.SolicitacaoPagador
                : (!string.IsNullOrWhiteSpace(request?.merchant?.Name) ? request.merchant.Name : "Pix cobrança");

            var serviceName = !string.IsNullOrWhiteSpace(request?.merchant?.Name) ? request.merchant.Name : "Pix";

            var dueDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var payForm = new[] { "PIX" };

            return new
            {
                code = txId,
                customer = new
                {
                    name = customerName,
                    email = "cliente@example.com",
                    document = new
                    {
                        identity = identity,
                        type = documentType
                    }
                },
                services = new object[]
                {
                    new
                    {
                        name = serviceName,
                        description = description,
                        amount = amountCents
                    }
                },
                payment_terms = new
                {
                    due_date = dueDate
                },
                payment_forms = payForm
            };
        }

        private static CoraInvoiceResponse DeserializeInvoice(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("Resposta vazia da Cora.");

            var invoice = JsonConvert.DeserializeObject<CoraInvoiceResponse>(responseBody);
            if (invoice == null || string.IsNullOrWhiteSpace(invoice.Id))
                throw new ArgumentException("Resposta inválida da Cora: " + responseBody);

            return invoice;
        }

        private static List<CoraInvoiceResponse> DeserializeInvoiceList(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("Resposta vazia da Cora.");

            var token = JToken.Parse(responseBody);

            var array = token as JArray;
            if (array != null)
                return array.ToObject<List<CoraInvoiceResponse>>() ?? new List<CoraInvoiceResponse>();

            var obj = token as JObject;
            if (obj != null)
            {
                var candidates = new[] { "invoices", "data", "items", "results" };
                foreach (var key in candidates)
                {
                    var a = obj[key] as JArray;
                    if (a != null)
                        return a.ToObject<List<CoraInvoiceResponse>>() ?? new List<CoraInvoiceResponse>();
                }
            }

            throw new ArgumentException("Resposta inválida da Cora: " + responseBody);
        }

        private static string GenerateQrCodeBase64(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            using (var ms = new MemoryStream())
            {
                var bw = new BarcodeWriter();
                var encOptions = new EncodingOptions() { Width = 200, Height = 200, Margin = 0 };
                bw.Options = encOptions;
                bw.Format = BarcodeFormat.QR_CODE;

                using (var bitmap = new Bitmap(bw.Write(text)))
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return Convert.ToBase64String(ms.GetBuffer());
                }
            }
        }

        private static string MapCoraStatusToCobStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return status;

            switch (status.Trim().ToUpperInvariant())
            {
                case "OPEN":
                case "IN_PAYMENT":
                case "DRAFT":
                case "LATE":
                    return "ATIVA";
                case "PAID":
                    return "CONCLUIDA";
                case "CANCELLED":
                    return "REMOVIDA_PELO_USUARIO_RECEBEDOR";
                default:
                    return status;
            }
        }

        private static Valor BuildValorFromTotalAmountCents(int? cents)
        {
            if (cents == null)
                return null;

            var decimalValue = cents.Value / 100m;
            return new Valor { Original = decimalValue.ToString("0.00", CultureInfo.InvariantCulture) };
        }

        private static Devedor BuildDevedorFromCustomer(string name, string docType, string identity)
        {
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(identity))
                return null;

            var digits = string.IsNullOrWhiteSpace(identity) ? null : new string(identity.Where(char.IsDigit).ToArray());
            var type = string.IsNullOrWhiteSpace(docType) ? null : docType.Trim().ToUpperInvariant();

            var devedor = new Devedor { Nome = name };
            if (type == "CNPJ" || (!string.IsNullOrWhiteSpace(digits) && digits.Length > 11))
                devedor.Cnpj = digits;
            else
                devedor.Cpf = digits;

            return devedor;
        }

        private class InvoiceCreateResult
        {
            public bool IsSuccessStatusCode { get; set; }
            public System.Net.HttpStatusCode StatusCode { get; set; }
            public string ResponseBody { get; set; }
        }

        private class CoraInvoiceResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("total_amount")]
            public int? TotalAmount { get; set; }

            [JsonProperty("customer")]
            public CoraCustomer Customer { get; set; }

            [JsonProperty("payment_options")]
            public CoraPaymentOptions PaymentOptions { get; set; }

            [JsonProperty("pix")]
            public CoraPix Pix { get; set; }
        }

        private class CoraCustomer
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("document")]
            public CoraDocument Document { get; set; }
        }

        private class CoraDocument
        {
            [JsonProperty("identity")]
            public string Identity { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }

        private class CoraPaymentOptions
        {
            [JsonProperty("bank_slip")]
            public CoraBankSlip BankSlip { get; set; }
        }

        private class CoraBankSlip
        {
            [JsonProperty("url")]
            public string Url { get; set; }
        }

        private class CoraPix
        {
            [JsonProperty("emv")]
            public string Emv { get; set; }
        }
    }
}

