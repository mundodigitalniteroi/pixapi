using Negocio.Extentions;
using Negocio.Models;
using Negocio.Requests.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Requests.RequestServices
{
    /// <summary>
    /// Integração Direta (mTLS + client_id) com a API da Cora para emissão de cobrança Pix (QR Code).
    /// </summary>
    public class CoraPixService
    {
        private const string DefaultTokenBaseUrl = "https://matls-clients.api.stage.cora.com.br";
        private const string DefaultApiBaseUrl = "https://api.stage.cora.com.br";

        public async Task<Cob> Create(string txId, CobRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Parametros == null)
                throw new ArgumentException("Parametros é obrigatório para integração com a Cora.");

            if (string.IsNullOrWhiteSpace(request.Parametros.ClientId))
                throw new ArgumentException("ClientId é obrigatório para integração com a Cora (Integração Direta).");

            X509Certificate2 certificate = null;

            // Prioriza leitura do arquivo se informado

                // Caminho relativo à raiz do projeto
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var certPath = "C:\\Repositories\\pixapi\\GeraPixMundoDigital\\Cora\\certificate.pem";
                //Path.Combine(baseDir, "..", "GeraPixMundoDigital", "CertificadosCora", request.Parametros.CertificateFileName);

                var certKeyPath = "C:\\Repositories\\pixapi\\GeraPixMundoDigital\\Cora\\private-key.key";

                //certPath = Path.GetFullPath(certPath);

                if (!File.Exists(certPath))
                    throw new ArgumentException($"Certificado não encontrado em: {certPath}");
                certificate = LoadCertificateWithKey(certPath, certKeyPath);

            var tokenBaseUrl = !string.IsNullOrWhiteSpace(request.Parametros.CoraTokenBaseUrl)
                ? request.Parametros.CoraTokenBaseUrl
                : DefaultTokenBaseUrl;

            var apiBaseUrl = !string.IsNullOrWhiteSpace(request.Parametros.CoraApiBaseUrl)
                ? request.Parametros.CoraApiBaseUrl
                : DefaultApiBaseUrl;

            var accessToken = await CreateAccessToken(tokenBaseUrl, request.Parametros.ClientId, certificate).ConfigureAwait(false);

            // A API de QR Code Pix da Cora é descrita na documentação, mas o schema completo pode variar por versão.
            // Por isso, tentamos dois formatos comuns com base nos nossos dados atuais (p.ex. payer/customer + services).
            var payloadA = BuildPayloadVariantA(txId, request);
            var payloadB = BuildPayloadVariantB(txId, request);

            var invoiceResult = await CreateInvoice(apiBaseUrl, accessToken, txId, payloadA).ConfigureAwait(false);

            // Fallback: se falhar com 4xx (tipicamente payload inválido), tenta segunda variante.
            if (!invoiceResult.IsSuccessStatusCode && (invoiceResult.StatusCode == System.Net.HttpStatusCode.BadRequest
                || (int)invoiceResult.StatusCode == 422))
            {
                invoiceResult = await CreateInvoice(apiBaseUrl, accessToken, txId, payloadB).ConfigureAwait(false);
            }

            if (!invoiceResult.IsSuccessStatusCode)
            {
                throw new ArgumentException(invoiceResult.ResponseBody ?? "Erro ao criar cobrança Pix na Cora.");
            }

            var qr = ExtractQrFromResponse(invoiceResult.ResponseBody);

            var cob = new Cob(request.Chave)
            {
                Txid = qr.Txid ?? txId,
                Status = qr.Status ?? "CREATED",
                QrTexto = qr.QrString,
                QrCode = qr.QrCodeBase64,

                // Mantém informações que já existem no nosso contrato interno.
                Valor = request.Valor,
                merchant = request.merchant,
                SolicitacaoPagador = request.SolicitacaoPagador
            };

            // Se a Cora retornou apenas a string (EMV/pix payload), geramos o QR image localmente.
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

        private async Task<string> CreateAccessToken(string tokenBaseUrl, string clientId, X509Certificate2 certificate)
        {

            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(certificate);

            // Mantém comportamento semelhante ao restante do repositório.
            // Observação: aceitar qualquer certificado é risco de segurança; ajustar em produção.
            // handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            using (var client = new HttpClient(handler))
            {
                var tokenUrl = tokenBaseUrl.TrimEnd('/') + "/token";

                var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "seuclientid"),
            });

                var response = await client.PostAsync(tokenUrl, formData);

                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Body: {body}");

                var token = JsonConvert.DeserializeObject<Token>(body);
                return token != null ? token.AccessToken : null;
            }
        }

        private async Task<InvoiceCreateResult> CreateInvoice(string apiBaseUrl, string accessToken, string txId, object payload)
        {
            var invoiceUrl = apiBaseUrl.TrimEnd('/') + "/invoices/";

            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, invoiceUrl))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                requestMessage.Headers.Add("Idempotency-Key", txId);
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

        private object BuildPayloadVariantA(string txId, CobRequest request)
        {
            var amount = request.Valor != null ? request.Valor.Original.ToDecimalUSCulture() : 0m;

            var payerName = request.Devedor != null ? request.Devedor.Nome : null;
            var documentType = request.Devedor != null && request.Devedor.IsCNPJ ? "CNPJ" : "CPF";
            var documentNumber = request.Devedor != null
                ? (request.Devedor.IsCNPJ ? request.Devedor.Cnpj : request.Devedor.Cpf)
                : null;

            var description = !string.IsNullOrWhiteSpace(request.SolicitacaoPagador)
                ? request.SolicitacaoPagador
                : (!string.IsNullOrWhiteSpace(request.merchant != null ? request.merchant.Name : null) ? request.merchant.Name : "Pix cobrança");

            // Alguns endpoints aceitam "expiration" em segundos.
            // Se a Cora exigir outra chave (p.ex. due_date), ajustamos no próximo iteration.
            var expirationSeconds = request.Calendario != null ? request.Calendario.Expiracao : (int?)null;

            return new
            {
                code = txId,
                amount = amount,
                description = description,
                expiration = expirationSeconds,
                payer = new
                {
                    name = payerName,
                    document = documentNumber,
                    document_type = documentType
                }
            };
        }

        private object BuildPayloadVariantB(string txId, CobRequest request)
        {
            var amount = request.Valor != null ? request.Valor.Original.ToDecimalUSCulture() : 0m;

            var documentType = request.Devedor != null && request.Devedor.IsCNPJ ? "CNPJ" : "CPF";
            var documentNumber = request.Devedor != null
                ? (request.Devedor.IsCNPJ ? request.Devedor.Cnpj : request.Devedor.Cpf)
                : null;

            var customerName = request.Devedor != null ? request.Devedor.Nome : null;
            var description = !string.IsNullOrWhiteSpace(request.SolicitacaoPagador)
                ? request.SolicitacaoPagador
                : (!string.IsNullOrWhiteSpace(request.merchant != null ? request.merchant.Name : null) ? request.merchant.Name : "Pix cobrança");

            var expirationSeconds = request.Calendario != null ? request.Calendario.Expiracao : (int?)null;

            return new
            {
                code = txId,
                expiration = expirationSeconds,
                customer = new
                {
                    name = customerName,
                    document = new
                    {
                        type = documentType,
                        number = documentNumber
                    }
                },
                services = new object[]
                {
                    new
                    {
                        name = request.merchant != null ? request.merchant.Name : "Pix",
                        description = description,
                        amount = amount
                    }
                }
            };
        }

        private QrExtractionResult ExtractQrFromResponse(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                throw new ArgumentException("Resposta vazia da Cora.");

            var obj = JsonConvert.DeserializeObject<JObject>(responseBody);
            if (obj == null)
                throw new ArgumentException("Resposta inválida da Cora: " + responseBody);

            var txid = FindFirstString(obj, "invoice_id", "id", "txid", "transaction_id", "code");
            var status = FindFirstString(obj, "status", "payment_status", "paymentStatus");

            // Tentativas de achar payload/QR string.
            var qrString = FindFirstString(obj,
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
                Txid = txid,
                Status = status,
                QrString = qrString,
                QrCodeBase64 = qrCodeBase64
            };
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

            // Quando é array, percorre elementos.
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

        private class InvoiceCreateResult
        {
            public bool IsSuccessStatusCode { get; set; }
            public System.Net.HttpStatusCode StatusCode { get; set; }
            public string ResponseBody { get; set; }
        }

        private class QrExtractionResult
        {
            public string Txid { get; set; }
            public string Status { get; set; }
            public string QrString { get; set; }
            public string QrCodeBase64 { get; set; }
        }

        static X509Certificate2 LoadCertificateWithKey(string certPath, string keyPath)
        {
            // Carrega o certificado PEM
            X509Certificate2 cert;
            using (var reader = File.OpenRead(certPath))
            {
                var pemReader = new PemReader(new StreamReader(reader));
                var certObject = pemReader.ReadObject();
                var bcCert = (Org.BouncyCastle.X509.X509Certificate)certObject;
                cert = new X509Certificate2(bcCert.GetEncoded());
            }

            // Carrega a chave privada PEM
            AsymmetricCipherKeyPair keyPair;
            using (var reader = new StreamReader(keyPath))
            {
                var pemReader = new PemReader(reader);
                keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
            }

            // Converte a chave BouncyCastle para RSA nativo do .NET
            var rsaParams = DotNetUtilities.ToRSAParameters(
                (RsaPrivateCrtKeyParameters)keyPair.Private);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);

            // Associa a chave privada ao certificado
            var certWithKey = cert.CopyWithPrivateKey(rsa); // .NET 4.6.2+
                                                            // Exporta e reimporta para garantir que o handler reconheça a chave
            return new X509Certificate2(
                certWithKey.Export(X509ContentType.Pkcs12),
                (string)null,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        }
    }

}

