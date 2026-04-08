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

            var qr = ExtractQrFromResponse(invoiceResult.ResponseBody);

            var cob = new Cob(request.Chave)
            {
                Txid = txId,
                Status = qr.Status ?? "CREATED",
                QrTexto = qr.QrString,
                QrCode = qr.QrCodeBase64,
                Referencia = qr.InvoiceId,

                Valor = request.Valor,
                merchant = request.merchant,
                SolicitacaoPagador = request.SolicitacaoPagador
            };

            if (string.IsNullOrWhiteSpace(cob.QrCode) && !string.IsNullOrWhiteSpace(qr.QrCodeUrl))
            {
                var url = NormalizeUrl(qr.QrCodeUrl);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var bytes = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
                        if (bytes != null && bytes.Length > 0)
                            cob.QrCode = Convert.ToBase64String(bytes);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(cob.QrCode) && !string.IsNullOrWhiteSpace(cob.QrTexto))
            {
                var cobRequestService = new CobRequestService();
                using (var ms = new MemoryStream())
                {
                    using (var bitmap = new Bitmap(cobRequestService.GerarQRCode(200, 200, cob.QrTexto)))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        cob.QrCode = Convert.ToBase64String(ms.GetBuffer());
                    }
                }
            }

            return cob;
        }

        public async Task<Cob> GetByReferencia(CobRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var invoiceId = request.Referencia;
            if (string.IsNullOrWhiteSpace(invoiceId))
                throw new ArgumentException("Referencia (id da invoice da Cora) não informado.");

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

            var qr = ExtractQrFromResponse(invoiceResult.ResponseBody);

            var cob = new Cob(request.Chave)
            {
                Txid = qr.Code ?? request.txId,
                Status = MapCoraStatusToCobStatus(qr.Status),
                QrTexto = qr.QrString,
                QrCode = qr.QrCodeBase64,
                Referencia = qr.InvoiceId ?? invoiceId,

                Valor = request.Valor ?? BuildValorFromTotalAmountCents(qr.TotalAmountCents),
                merchant = request.merchant,
                SolicitacaoPagador = request.SolicitacaoPagador,
                Devedor = request.Devedor ?? qr.Devedor
            };

            if (string.IsNullOrWhiteSpace(cob.QrCode) && !string.IsNullOrWhiteSpace(qr.QrCodeUrl))
            {
                var url = NormalizeUrl(qr.QrCodeUrl);
                if (!string.IsNullOrWhiteSpace(url))
                {
                    using (var httpClient = new HttpClient())
                    using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var mediaType = response.Content?.Headers?.ContentType?.MediaType;
                            if (!string.IsNullOrWhiteSpace(mediaType) && mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                            {
                                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                                if (bytes != null && bytes.Length > 0)
                                    cob.QrCode = Convert.ToBase64String(bytes);
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(cob.QrCode) && !string.IsNullOrWhiteSpace(cob.QrTexto))
            {
                var cobRequestService = new CobRequestService();
                using (var ms = new MemoryStream())
                {
                    using (var bitmap = new Bitmap(cobRequestService.GerarQRCode(200, 200, cob.QrTexto)))
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        cob.QrCode = Convert.ToBase64String(ms.GetBuffer());
                    }
                }
            }

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

            var root = JsonConvert.DeserializeObject<JToken>(invoicesResult.ResponseBody);
            if (root == null)
                throw new ArgumentException("Resposta inválida da Cora: " + invoicesResult.ResponseBody);

            var items = ExtractFirstArray(root);
            var cobs = new List<Cob>();

            foreach (var item in items)
            {
                var obj = item as JObject;
                if (obj == null)
                    continue;

                var qr = ExtractQrFromResponse(obj.ToString(Formatting.None));

                cobs.Add(new Cob(request.Chave)
                {
                    Txid = qr.Code ?? qr.InvoiceId,
                    Status = MapCoraStatusToCobStatus(qr.Status),
                    QrTexto = qr.QrString,
                    QrCode = qr.QrCodeBase64,
                    Referencia = qr.InvoiceId,
                    Valor = BuildValorFromTotalAmountCents(qr.TotalAmountCents),
                    Devedor = qr.Devedor
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

        private QrExtractionResult ExtractQrFromResponse(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("Resposta vazia da Cora.");

            var obj = JsonConvert.DeserializeObject<JObject>(responseBody);
            if (obj == null)
                throw new ArgumentException("Resposta inválida da Cora: " + responseBody);

            var invoiceId = FindFirstString(obj, "invoice_id", "id");
            var code = FindFirstString(obj, "code", "txid", "transaction_id");
            var status = FindFirstString(obj, "status", "payment_status", "paymentStatus");

            var totalAmountCents = GetIntByPath(obj, "total_amount");

            var customerName = GetStringByPath(obj, "customer.name");
            var customerDocIdentity = GetStringByPath(obj, "customer.document.identity");
            var customerDocType = GetStringByPath(obj, "customer.document.type");
            var devedor = BuildDevedorFromCustomer(customerName, customerDocType, customerDocIdentity);

            var qrUrl = GetStringByPath(obj, "payment_options.bank_slip.url")
                ?? GetStringByPath(obj, "pix.bank_slip.url")
                ?? GetStringByPath(obj, "payment_options.pix.url")
                ?? GetStringByPath(obj, "pix.qr_code.url")
                ?? GetStringByPath(obj, "pix.qrcode.url")
                ?? GetStringByPath(obj, "pix.url");

            // Tentativas de achar payload/QR string.
            var qrString = GetStringByPath(obj, "pix.emv")
                ?? GetStringByPath(obj, "payment_options.pix.emv")
                ?? FindFirstString(obj,
                "qr_string",
                "qrstring",
                "qrCodeString",
                "pix_copy_and_paste",
                "pix_copy_and_paste_string",
                "pix_copia_e_colar",
                "payload",
                "emv"
            );

            // Tentativas de base64.
            var qrCodeBase64 = FindFirstString(obj,
                "qr_code_base64",
                "qrCodeBase64",
                "qrCode",
                "qr_code"
            );

            // Heurística: se "qrCodeBase64" parece EMV (começa com 000201), trata como qrString.
            if (!string.IsNullOrWhiteSpace(qrCodeBase64) && LooksLikeEmv(qrCodeBase64) && string.IsNullOrWhiteSpace(qrString))
            {
                qrString = qrCodeBase64;
                qrCodeBase64 = null;
            }

            // Caso base64 venha com prefixo data:image/..;base64,
            if (!string.IsNullOrWhiteSpace(qrCodeBase64) && qrCodeBase64.IndexOf(",", StringComparison.Ordinal) >= 0 && qrCodeBase64.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                qrCodeBase64 = qrCodeBase64.Substring(qrCodeBase64.IndexOf(",") + 1);
            }

            // Se só achou base64 mas qrString está vazio, mantemos só base64.
            return new QrExtractionResult
            {
                InvoiceId = invoiceId,
                Code = code,
                Status = status,
                QrString = qrString,
                QrCodeBase64 = qrCodeBase64,
                QrCodeUrl = qrUrl,
                TotalAmountCents = totalAmountCents,
                Devedor = devedor
            };
        }

        private static string GetStringByPath(JObject obj, string jsonPath)
        {
            if (obj == null || string.IsNullOrWhiteSpace(jsonPath))
                return null;

            var token = obj.SelectToken(jsonPath);
            if (token == null || token.Type != JTokenType.String)
                return null;

            var value = token.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static int? GetIntByPath(JObject obj, string jsonPath)
        {
            if (obj == null || string.IsNullOrWhiteSpace(jsonPath))
                return null;

            var token = obj.SelectToken(jsonPath);
            if (token == null)
                return null;

            if (token.Type == JTokenType.Integer)
                return token.Value<int>();

            if (token.Type == JTokenType.Float)
                return (int)Math.Round(token.Value<double>(), MidpointRounding.AwayFromZero);

            if (token.Type == JTokenType.String)
            {
                int parsed;
                if (int.TryParse(token.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                    return parsed;
            }

            return null;
        }

        private static JArray ExtractFirstArray(JToken token)
        {
            if (token == null)
                return new JArray();

            var arr = token as JArray;
            if (arr != null)
                return arr;

            var obj = token as JObject;
            if (obj != null)
            {
                var candidates = new[] { "invoices", "data", "items", "results" };
                foreach (var c in candidates)
                {
                    var t = obj[c];
                    var a = t as JArray;
                    if (a != null)
                        return a;
                }

                foreach (var prop in obj.Properties())
                {
                    var a = prop.Value as JArray;
                    if (a != null)
                        return a;
                }
            }

            return new JArray();
        }

        private static string NormalizeUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var cleaned = new string(value.Where(c => !char.IsWhiteSpace(c) && c != '`' && c != '"').ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private static string FindFirstString(JToken token, params string[] keys)
        {
            if (token == null)
                return null;

            var keySet = new HashSet<string>(keys ?? new string[0], StringComparer.OrdinalIgnoreCase);
            return FindFirstStringRecursive(token, keySet);
        }

        private static string FindFirstStringRecursive(JToken token, HashSet<string> keySet)
        {
            if (token == null)
                return null;

            // Quando é objeto com propriedades, checa chave/valor.
            var obj = token as JObject;
            if (obj != null)
            {
                foreach (var prop in obj.Properties())
                {
                    if (keySet.Contains(prop.Name) && prop.Value != null && prop.Value.Type == JTokenType.String)
                        return prop.Value.ToString();

                    var nested = FindFirstStringRecursive(prop.Value, keySet);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }

            var arr = token as JArray;
            if (arr != null)
            {
                foreach (var item in arr)
                {
                    var nested = FindFirstStringRecursive(item, keySet);
                    if (!string.IsNullOrWhiteSpace(nested))
                        return nested;
                }
            }

            return null;
        }

        private static bool LooksLikeEmv(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // EMV Pix geralmente começa com "000201" e contém "br.gov.bcb.pix".
            return value.StartsWith("000201", StringComparison.OrdinalIgnoreCase)
                || value.IndexOf("br.gov.bcb.pix", StringComparison.OrdinalIgnoreCase) >= 0;
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

        private class QrExtractionResult
        {
            public string InvoiceId { get; set; }
            public string Code { get; set; }
            public string Status { get; set; }
            public string QrString { get; set; }
            public string QrCodeBase64 { get; set; }
            public string QrCodeUrl { get; set; }
            public int? TotalAmountCents { get; set; }
            public Devedor Devedor { get; set; }
        }
    }
}

