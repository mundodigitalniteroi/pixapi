using NegocioCielo.Helper;
using NegocioCielo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NegocioCielo
{
    public class CieloApi
    {
        private readonly static HttpClient _http = new HttpClient();

        public Dictionary<string, string> statusRetorno = new Dictionary<string, string>();
        public Dictionary<string, string> StatusCancelamento = new Dictionary<string, string>();

        /// <summary>
        /// Tempo para TimeOut da requisição, por default é 60 segundos
        /// </summary>
        private readonly int _timeOut = 0;

        public Merchant Merchant { get; }
        public ISerializerJSON SerializerJSON { get; }
        public CieloEnvironment Environment { get; }

        static CieloApi()
        {
            _http.DefaultRequestHeaders.ExpectContinue = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="merchant"></param>
        /// <param name="serializer">Crie o seu Provider Json</param>
        /// <param name="timeOut">Tempo para TimeOut da requisição, por default é 60 segundos</param>
        public CieloApi(CieloEnvironment environment, Merchant merchant, ISerializerJSON serializer, int timeOut = 60000, SecurityProtocolType securityProtocolType = SecurityProtocolType.Tls12)
        {
            Environment = environment;
            Merchant = merchant;
            SerializerJSON = serializer;            
            _timeOut = timeOut;

            ServicePointManager.SecurityProtocol = securityProtocolType;

            CarregarDicionarios();
            
        }

        private void CarregarDicionarios()
        {
            statusRetorno.Add("00", "Transação autorizada com sucesso. ");
            statusRetorno.Add("02", "Transação não autorizada. Transação referida. ");
            statusRetorno.Add("09", "Transação cancelada parcialmente com sucesso. ");
            statusRetorno.Add("11", "Transação autorizada com sucesso para cartão emitido no exterior ");
            statusRetorno.Add("21", "Cancelamento não efetuado. Transação não localizada. ");
            statusRetorno.Add("22", "Parcelamento inválido. Número de parcelas inválidas. ");
            statusRetorno.Add("24", "Quantidade de parcelas inválido. ");
            statusRetorno.Add("60", "Transação não autorizada. ");
            statusRetorno.Add("67", "Transação não autorizada. Cartão bloqueado para compras hoje. ");
            statusRetorno.Add("70", "Transação não autorizada. Limite excedido/sem saldo. ");
            statusRetorno.Add("72", "Cancelamento não efetuado. Saldo disponível para cancelamento insuficiente. ");
            statusRetorno.Add("80", "Transação não autorizada. Divergencia na data de transação/pagamento. ");
            statusRetorno.Add("83", "Transação não autorizada. Erro no controle de senhas ");
            statusRetorno.Add("85", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("89", "Erro na transação. ");
            statusRetorno.Add("90", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("97", "Valor não permitido para essa transação. ");
            statusRetorno.Add("98", "Sistema/comunicação indisponível. ");
            statusRetorno.Add("475", "Timeout de Cancelamento ");
            statusRetorno.Add("999", "Sistema/comunicação indisponível. ");
            statusRetorno.Add("AA", "Tempo Excedido ");
            statusRetorno.Add("AF", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("AG", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("AH", "Transação não permitida. Cartão de crédito sendo usado com débito. Use a função crédito. ");
            statusRetorno.Add("AI", "Transação não autorizada. Autenticação não foi realizada. ");
            statusRetorno.Add("AJ", "Transação não permitida. Transação de crédito ou débito em uma operação que permite apenas Private Label. Tente novamente selecionando a opção Private Label. ");
            statusRetorno.Add("AV", "Transação não autorizada. Dados Inválidos ");
            statusRetorno.Add("BD", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("BL", "Transação não autorizada. Limite diário excedido. ");
            statusRetorno.Add("BM", "Transação não autorizada. Cartão Inválido ");
            statusRetorno.Add("BN", "Transação não autorizada. Cartão ou conta bloqueado. ");
            statusRetorno.Add("BO", "Transação não permitida. Falha da operação. ");
            statusRetorno.Add("BP", "Transação não autorizada. Conta corrente inexistente. ");
            statusRetorno.Add("BP176", "Transação não permitida. ");
            statusRetorno.Add("C1", "Transação não permitida. Cartão não pode processar transações de débito. ");
            statusRetorno.Add("C2", "Transação não permitida. ");
            statusRetorno.Add("C3", "Transação não permitida. ");
            statusRetorno.Add("CF", "Transação não autorizada.C79:J79 Falha na validação dos dados. ");
            statusRetorno.Add("CG", "Transação não autorizada. Falha na validação dos dados. ");
            statusRetorno.Add("DF", "Transação não permitida. Falha no cartão ou cartão inválido. ");
            statusRetorno.Add("DM", "Transação não autorizada. Limite excedido/sem saldo. ");
            statusRetorno.Add("DQ", "Transação não autorizada. Falha na validação dos dados. ");
            statusRetorno.Add("DS", "Transação não permitida para o cartão ");
            statusRetorno.Add("EB", "Transação não autorizada. Limite diário excedido. ");
            statusRetorno.Add("EE", "Transação não permitida. Valor da parcela inferior ao mínimo permitido. ");
            statusRetorno.Add("EK", "Transação não permitida para o cartão ");
            statusRetorno.Add("FC", "Transação não autorizada. Ligue Emissor ");
            statusRetorno.Add("FE", "Transação não autorizada. Divergencia na data de transação/pagamento. ");
            statusRetorno.Add("FF", "Cancelamento OK ");
            statusRetorno.Add("FG", "Transação não autorizada. Ligue AmEx 08007285090. ");
            statusRetorno.Add("GA", "Aguarde Contato ");
            statusRetorno.Add("GD", "Transação não permitida. ");
            statusRetorno.Add("HJ", "Transação não permitida. Código da operação inválido. ");
            statusRetorno.Add("IA", "Transação não permitida. Indicador da operação inválido. ");
            statusRetorno.Add("KA", "Transação não permitida. Falha na validação dos dados. ");
            statusRetorno.Add("KB", "Transação não permitida. Selecionado a opção incorrente. ");
            statusRetorno.Add("KE", "Transação não autorizada. Falha na validação dos dados. ");
            statusRetorno.Add("N7", "Transação não autorizada. Código de segurança inválido. ");
            statusRetorno.Add("U3", "Transação não permitida. Falha na validação dos dados. ");
            statusRetorno.Add("BR", "Transação não autorizada. Conta encerrada ");
            statusRetorno.Add("46", "Transação não autorizada. Conta encerrada ");
            statusRetorno.Add("6P", "Transação não autorizada. Dados Inválidos ");

            StatusCancelamento.Add("6", "Solicitação de cancelamento parcial aprovada com sucesso");
            StatusCancelamento.Add("9", "Solicitação de cancelamento total aprovada com sucesso");
            StatusCancelamento.Add("72", "Erro: Saldo do lojista insuficiente para cancelamento de venda");
            StatusCancelamento.Add("77", "Erro: Venda original não encontrada para cancelamento");
            StatusCancelamento.Add("100", "Erro: Forma de pagamento e/ou Bandeira não permitem cancelamento");
            StatusCancelamento.Add("101", "Erro: Valor de cancelamento solicitado acima do prazo permitido para cancelar");
            StatusCancelamento.Add("102", "Erro: Cancelamento solicitado acima do valor da transação original");
            StatusCancelamento.Add("103", "Restrição Cadastral. Cancelamento não permitido. Entre em contato com a Central de Cancelamento");
            StatusCancelamento.Add("104", "Restrição Cadastral. Cancelamento não permitido. Entre em contato com a Central de Cancelamento");
            StatusCancelamento.Add("105", "Restrição Cadastral. Cancelamento não permitido. Entre em contato com a Central de Cancelamento");
            StatusCancelamento.Add("106", "Restrição Cadastral. Cancelamento não permitido. Entre em contato com a Central de Cancelamento");
            StatusCancelamento.Add("107", "Restrição Cadastral. Cancelamento não permitido. Entre em contato com a Central de Cancelamento");
            StatusCancelamento.Add("108", "Erro: Número do Estabelecimento (EC) não encontrado. Por favor, verifique o número enviado");
            StatusCancelamento.Add("475", "Falha no processamento. Por favor, tente novamente");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchant"></param>
        /// <param name="serializer">Crie o seu Provider Json</param>
        /// <param name="timeOut">Tempo para TimeOut da requisição, por default é 60 segundos</param>
        public CieloApi(Merchant merchant, ISerializerJSON serializer, int timeOut = 60000)
                : this(CieloEnvironment.PRODUCTION, merchant, serializer, timeOut) { }

        private IDictionary<string, string> GetHeaders(Guid requestId)
        {
            return new Dictionary<string, string>(1)
            {
                { "RequestId", requestId.ToString() }
            };
        }

        private async Task<HttpResponseMessage> CreateRequestAsync(string resource, Method method = Method.GET, IDictionary<string, string> headers = null)
        {
            return await CreateRequestAsync<object>(resource, null, method, headers);
        }

        private async Task<HttpResponseMessage> CreateRequestAsync<T>(string resource, T value, Method method = Method.POST, IDictionary<string, string> headers = null)
        {
            StringContent content = null;

            if (value != null)
            {
                string json = SerializerJSON.Serialize<T>(value);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            return await ExecuteAsync(resource, headers, method, content);
        }

        private async Task<HttpResponseMessage> ExecuteAsync(string fullUrl, IDictionary<string, string> headers = null, Method method = Method.POST, StringContent content = null)
        {
            if (headers == null)
            {
                headers = new Dictionary<string, string>(2);
            }

            headers.Add("MerchantId", Merchant.Id.ToString());
            headers.Add("MerchantKey", Merchant.Key);

            var tokenSource = new CancellationTokenSource(_timeOut);

            try
            {
                HttpMethod httpMethod = null;

                if (method == Method.POST)
                {
                    httpMethod = HttpMethod.Post;
                    if (headers != null)
                    {
                        foreach (var item in headers)
                        {
                            content.Headers.Add(item.Key, item.Value);
                        }
                    }
                }
                else if (method == Method.GET)
                {
                    httpMethod = HttpMethod.Get;
                }
                else if (method == Method.PUT)
                {
                    httpMethod = HttpMethod.Put;
                }
                else if (method == Method.DELETE)
                {
                    httpMethod = HttpMethod.Delete;
                }

                var request = GetExecute(fullUrl, headers, httpMethod, content);
                return await _http.SendAsync(request, tokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                throw new CancellationTokenException(e);
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        private static HttpRequestMessage GetExecute(string fullUrl, IEnumerable<KeyValuePair<string, string>> headers, HttpMethod method, StringContent content = null)
        {
            var request = new HttpRequestMessage(method, fullUrl)
            {
                Content = content
            };

            if (method != HttpMethod.Post)
            {
                foreach (var item in headers)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }

            return request;
        }
        private async Task<T> GetResponseAsync<T>(HttpResponseMessage response)
        {
            await CheckResponseAsync(response);

            return SerializerJSON.Deserialize<T>(response.Content);
        }

        private async Task CheckResponseAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new CieloException($"Error code {response.StatusCode}.", result, this.SerializerJSON);
            }
        }

        /// <summary>
        /// Envia uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transaction> CreateTransactionAsync(Guid requestId, Transaction transaction)
        {
            if (transaction != null &&
                transaction.Customer != null &&
                transaction.Customer.Address != null)
            {
                var error = transaction.Customer.Address.IsValid();
                if (!string.IsNullOrEmpty(error))
                {
                    throw new CieloException(error, "1");
                }
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(Environment.GetTransactionUrl("/1/sales/"), transaction, Method.POST, headers);

            return await GetResponseAsync<Transaction>(response);
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransactionAsync(Guid paymentId)
        {
            return await GetTransactionAsync(Guid.NewGuid(), paymentId);
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransactionAsync(Guid requestId, Guid paymentId)
        {
            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/sales/{paymentId}"), Method.GET, headers);

            return await GetResponseAsync<Transaction>(response);
        }


        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<ReturnRecurrentPayment> GetRecurrentPaymentAsync(Guid requestId, Guid recurrentPaymentId)
        {
            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/RecurrentPayment/{recurrentPaymentId}"), Method.GET, headers);

            return await GetResponseAsync<ReturnRecurrentPayment>(response);
        }

        /// <summary>
        /// Cancela uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CancelTransactionAsync(Guid requestId, Guid paymentId, decimal? amount = null)
        {
            var url = Environment.GetTransactionUrl($"/1/sales/{paymentId}/void");

            if (amount.HasValue)
            {
                url += $"?Amount={NumberHelper.DecimalToInteger(amount)}";
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            return await GetResponseAsync<ReturnStatus>(response);
        }

        /// <summary>
        /// Captura uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <param name="serviceTaxAmount"></param>
        /// <returns></returns>
        public async Task<ReturnStatus> CaptureTransactionAsync(Guid requestId, Guid paymentId, decimal? amount = null, decimal? serviceTaxAmount = null)
        {
            var url = Environment.GetTransactionUrl($"/1/sales/{paymentId}/capture");

            if (amount.HasValue)
            {
                url += $"?Amount={NumberHelper.DecimalToInteger(amount)}";
            }

            if (serviceTaxAmount.HasValue)
            {
                url += $"?SeviceTaxAmount={NumberHelper.DecimalToInteger(serviceTaxAmount)}";
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            return await GetResponseAsync<ReturnStatus>(response);
        }

        /// <summary>
        /// Ativa uma recorrência
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns></returns>
        public async Task<bool> ActivateRecurrentAsync(Guid requestId, Guid recurrentPaymentId)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, true);
        }

        /// <summary>
        /// Desativa uma recorrência
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns></returns>
        public async Task<bool> DeactivateRecurrentAsync(Guid requestId, Guid recurrentPaymentId)
        {
            return await ManagerRecurrent(requestId, recurrentPaymentId, false);
        }

        /// <summary>
        /// Cria uma Token de um cartão válido ou não.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <param name="active">Parâmetro que define se uma recorrência será desativada ou ativada novamente</param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns>Se retornou true é porque a operação foi realizada com sucesso</returns>
        public async Task<ReturnStatusLink> CreateTokenAsync(Guid requestId, Card card)
        {
            card.CustomerName = card.Holder;
            card.SecurityCode = string.Empty;

            var url = Environment.GetTransactionUrl($"/1/Card");
            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, card, Method.POST, headers);

            //Se tiver errado será levantado uma exceção
            return await GetResponseAsync<ReturnStatusLink>(response);
        }

        /// <summary>
        /// Faz pagamento de 1 real e cancela logo em seguida para testar se o cartão é válido.
        /// Gera token somente de cartão válido
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public async Task<ReturnStatusLink> CreateTokenValidAsync(Guid requestId, Card creditCard, string softDescriptor = "Validando", Currency currency = Currency.BRL)
        {
            creditCard.SaveCard = true;
            creditCard.CustomerName = creditCard.Holder;

            var customer = new Customer(creditCard.CustomerName);

            var payment = new Payment(amount: 1,
                                      currency: currency,
                                      paymentType: PaymentType.CreditCard,
                                      installments: 1,
                                      capture: true,
                                      recurrentPayment: null,
                                      softDescriptor: softDescriptor,
                                      card: creditCard,
                                      returnUrl: string.Empty);

            var transaction = new Transaction(Guid.NewGuid().ToString(), customer, payment);

            var result = await CreateTransactionAsync(requestId, transaction);
            var status = result.Payment.GetStatus();
            if (status == Status.Authorized || status == Status.PaymentConfirmed)
            {
                //Cancelando pagamento de 1 REAL
                var resultCancel = await CancelTransactionAsync(Guid.NewGuid(), result.Payment.PaymentId.Value, 1);
                var status2 = resultCancel.GetStatus();
                if (status2 != Status.Voided)
                {
                    return new ReturnStatusLink
                    {
                        ReturnCode = resultCancel.ReturnCode,
                        ReturnMessage = resultCancel.ReturnMessage,
                        Status = resultCancel.Status,
                        Links = resultCancel.Links.FirstOrDefault()
                    };
                }
            }
            else
            {
                return new ReturnStatusLink
                {
                    ReturnCode = result.Payment.ReturnCode,
                    ReturnMessage = result.Payment.ReturnMessage,
                    Status = result.Payment.Status,
                    Links = result.Payment.Links.FirstOrDefault()
                };
            }

            var token = result.Payment.CreditCard.CardToken.HasValue ? result.Payment.CreditCard.CardToken.Value.ToString() : string.Empty;
            var statusLink = new ReturnStatusLink
            {
                CardToken = token,
                ReturnCode = result.Payment.ReturnCode,
                ReturnMessage = result.Payment.ReturnMessage,
                Status = result.Payment.Status,
                Links = result.Payment.Links.FirstOrDefault()
            };

            return statusLink;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="recurrentPaymentId"></param>
        /// <param name="active">Parâmetro que define se uma recorrência será desativada ou ativada novamente</param>
        /// <exception cref="CieloException">Ocorreu algum erro ao tentar alterar a recorrência</exception>
        /// <returns>Se retornou true é porque a operação foi realizada com sucesso</returns>
        private async Task<bool> ManagerRecurrent(Guid requestId, Guid recurrentPaymentId, bool active)
        {
            var url = Environment.GetTransactionUrl($"/1/RecurrentPayment/{recurrentPaymentId}/Deactivate");

            if (active)
            {
                //Ativar uma recorrência novamente
                url = Environment.GetTransactionUrl($"/1/RecurrentPayment/{recurrentPaymentId}/Reactivate");
            }

            var headers = GetHeaders(requestId);
            var response = await CreateRequestAsync(url, Method.PUT, headers);

            //Se tiver errado será levantado uma exceção
            await CheckResponseAsync(response);

            return true;
        }

        #region Método Sincronos

        /// <summary>
        ///  Cria uma Token de um cartão válido ou não.
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public ReturnStatusLink CreateToken(Guid requestId, Card card)
        {
            return RunTask(() =>
            {
                return CreateTokenAsync(requestId, card);
            });
        }

        public ReturnRecurrentPayment GetRecurrentPayment(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return GetRecurrentPaymentAsync(requestId, recurrentPaymentId);
            });
        }

        public ReturnMerchandOrderID GetMerchandOrderID(string merchantOrderId)
        {
            return RunTask(() =>
            {
                return GetMerchandOrderIDAsync(merchantOrderId);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<ReturnMerchandOrderID> GetMerchandOrderIDAsync(string merchantOrderId)
        {
            var headers = GetHeaders(Guid.NewGuid());
            var response = await CreateRequestAsync(Environment.GetQueryUrl($"/1/sales?merchantOrderId={merchantOrderId}"), Method.GET, headers);

            return await GetResponseAsync<ReturnMerchandOrderID>(response);
        }

        /// <summary>
        /// Faz pagamento de 1 real e cancela logo em seguida para testar se o cartão é válido.
        /// Gera token somente de cartão válido
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="creditCard"></param>
        /// <param name="softDescriptor"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        public ReturnStatusLink CreateTokenValid(Guid requestId, Card creditCard, string softDescriptor = "Validando", Currency currency = Currency.BRL)
        {
            return RunTask(() =>
            {
                return CreateTokenValidAsync(requestId, creditCard, softDescriptor, currency);
            });
        }

        /// <summary>
        /// Captura uma transação (parcial ou total)
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <param name="amount"></param>
        /// <param name="serviceTaxAmount"></param>
        /// <returns></returns>
        public ReturnStatus CaptureTransaction(Guid requestId, Guid paymentId, decimal? amount = null, decimal? serviceTaxAmount = null)
        {
            return RunTask(() =>
            {
                return CaptureTransactionAsync(requestId, paymentId, amount, serviceTaxAmount);
            });
        }

        public bool ActivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return ActivateRecurrentAsync(requestId, recurrentPaymentId);
            });
        }

        public bool DeactivateRecurrent(Guid requestId, Guid recurrentPaymentId)
        {
            return RunTask(() =>
            {
                return DeactivateRecurrentAsync(requestId, recurrentPaymentId);
            });
        }

        public async Task<ReturnStatus> CancelTransaction(Guid requestId, Guid paymentId, decimal? amount = null)
        {
            return RunTask(() =>
            {
                return CancelTransactionAsync(requestId, paymentId, amount);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public Transaction GetTransaction(Guid requestId, Guid paymentId)
        {
            return RunTask(() =>
            {
                return GetTransactionAsync(requestId, paymentId);
            });
        }

        /// <summary>
        /// Consulta uma transação
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public async Task<Transaction> GetTransaction(Guid paymentId)
        {
            return RunTask(() =>
            {
                return GetTransactionAsync(paymentId);
            });
        }

        /// <summary>
        /// Envia uma transação
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<Transaction> CreateTransaction(Guid requestId, Transaction transaction)
        {
            return RunTask(() =>
            {
                return CreateTransactionAsync(requestId, transaction);
            });
        }

        private static TResult RunTask<TResult>(Func<Task<TResult>> method)
        {
            try
            {
                return Task.Run(method).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e.InnerException is CieloException ex)
                {
                    throw ex;
                }
                else if (e is CieloException exCielo)
                {
                    throw exCielo;
                }

                throw e;
            }
        }
        #endregion



        #region ChamadasAPi
        public async Task<RetornoCielo> GerarTransacaoComCaptura(EntradaPagamentoCielo obj)
        {
            var customer = new Customer(name: obj.dadoscartao.nome_cartao);
            RetornoCielo retorno = new RetornoCielo();

            var creditCard = new Card(
                cardNumber: obj.dadoscartao.numero_cartao,
                holder: obj.dadoscartao.nome_cartao,
                expirationDate: Convert.ToDateTime("01/" + obj.dadoscartao.validade_cartao),
                securityCode: obj.dadoscartao.cod_seguranca.ToString(),
                brand: (CardBrand)Enum.Parse(typeof(CardBrand), obj.dadoscartao.bandeira_cartao, true));

            var payment = new Payment(
                amount: obj.dadoscobranca.valor,
                currency: Currency.BRL,
                installments: obj.dadoscobranca.parcelas,
                capture: false,
                softDescriptor: obj.dadoscobranca.descricaoBreve,
                card: creditCard);

            var merchantOrderId = obj.id_pagamento;

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customer,
                payment: payment);

            var result = await CreateTransaction(Guid.NewGuid(), transaction);

            string ret;
            if (result.Payment.GetStatus() == Status.Authorized)
            {
                var captureTransaction = CaptureTransaction(Guid.NewGuid(), result.Payment.PaymentId.Value);
                retorno.id_pagamento = result.Payment.PaymentId.Value.ToString();                
                
                statusRetorno.TryGetValue(result.Payment.ReturnCode, out ret);
                retorno.statusTransacao = ret;
                retorno.tid = result.Payment.Tid;
                retorno.cod_autorizacao = result.Payment.AuthorizationCode;
                retorno.proofOfSales = result.Payment.ProofOfSale;
                
               
                retorno.statusCaptura = Enum.Parse(typeof(Status), captureTransaction.Status, true).ToString(); // captureTransaction.GetStatus().ToString();
                retorno.sucesso = true;
            }
            else
            {
                retorno.id_pagamento = result.Payment.PaymentId.Value.ToString();
                
                statusRetorno.TryGetValue(result.Payment.ReturnCode, out ret);

                if(string.IsNullOrEmpty(ret))
                   retorno.statusTransacao = result.Payment.ReturnMessage;
                else
                    retorno.statusTransacao = ret;


                //retorno.transacao = result;
                retorno.sucesso = false;
            }

            return retorno;
        }

        public async Task<RetornoCielo> CancelarTransacao(string id_pagamento)
        {
            RetornoCielo retorno = new RetornoCielo();
            string ret;

            try
            {
                var cancelationTransaction = await CancelTransaction(Guid.NewGuid(), Guid.Parse(id_pagamento));

                StatusCancelamento.TryGetValue(cancelationTransaction.ProviderReturnCode, out ret);
                
                retorno.statusTransacao = ret;
                retorno.statusCaptura = cancelationTransaction.GetStatus().ToString();

                if (cancelationTransaction.ProviderReturnCode == "9" || cancelationTransaction.ProviderReturnCode == "6")
                { 
                    retorno.sucesso = true;
                }
                else
                    retorno.sucesso = false;

            }
            catch (Exception ex)
            {
                retorno.sucesso = false;
                throw;
            }            

            return retorno;

        }

        public async Task<RetornoCielo> ConsultarTransacao(string id_pagamento)
        {
            RetornoCielo retorno = new RetornoCielo();

            try
            {
                var Transaction = await GetTransaction(Guid.Parse(id_pagamento));
                retorno.transacao = Transaction;
                retorno.sucesso = true;
            }
            catch (Exception ex)
            {
                retorno.sucesso = false;
                throw;
            }

            return retorno;
        }
        #endregion
    }
}