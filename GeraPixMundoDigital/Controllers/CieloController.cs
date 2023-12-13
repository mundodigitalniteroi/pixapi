using Negocio.Models;
using NegocioCielo;
using NegocioCielo.Models;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace GeraPixMundoDigital.Controllers
{
    [RoutePrefix("api/cielo")]
    public class CieloController : ApiController
    {

        [AcceptVerbs("POST")]
        [Route("RealizarPagamento")]
        public async Task<RetornoCielo> RealizarPagamento(EntradaPagamentoCielo _obj)
        {
            ISerializerJSON json = new SerializerJSON();

            CieloApi _request;

            if (ConfigurationManager.AppSettings["ambienteProdutivo"].ToString().Equals("true"))
            {
                Merchant credenciais = new Merchant(Guid.Parse("d22304f3-81db-43f5-a9da-b68ae96fc20e"), "svbb4w4wo9nPqB7Xx6hYQz9tbjdKFUL2izenJgMM");
                //Merchant credenciais = new Merchant(Guid.Parse("e3c6da09-9a36-4f63-8dbe-28aca4899724"), "73Prs2DVvQNY2SOYI0UdAmKB/6cjxHbfncGXPeCNA2A=");
                
                _request = new CieloApi(CieloEnvironment.PRODUCTION, credenciais, json);
            }
            else
                _request = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);

            var cb = await _request.GerarTransacaoComCaptura(_obj);    

            return cb;
        }

        [AcceptVerbs("GET")]
        [Route("CancelarPagamento")]
        public async Task<RetornoCielo> CancelarPagamento(string IdPayment)
        {
            ISerializerJSON json = new SerializerJSON();

            CieloApi _request;

            if (ConfigurationManager.AppSettings["ambienteProdutivo"].ToString().Equals("true"))
            {
                Merchant credenciais = new Merchant(Guid.Parse("d22304f3-81db-43f5-a9da-b68ae96fc20e"), "svbb4w4wo9nPqB7Xx6hYQz9tbjdKFUL2izenJgMM");
                //Merchant credenciais = new Merchant(Guid.Parse("e3c6da09-9a36-4f63-8dbe-28aca4899724"), "73Prs2DVvQNY2SOYI0UdAmKB/6cjxHbfncGXPeCNA2A=");


                _request = new CieloApi(CieloEnvironment.PRODUCTION, credenciais, json);
            }
            else
                _request = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);

            var cb = await _request.CancelarTransacao(IdPayment);

            return cb;
        }

        [AcceptVerbs("GET")]
        [Route("ConsultarTransacao")]
        public async Task<RetornoCielo> ConsultarTransacao(string IdPayment)
        {
            ISerializerJSON json = new SerializerJSON();

            CieloApi _request;

            if (ConfigurationManager.AppSettings["ambienteProdutivo"].ToString().Equals("true"))
            {
                Merchant credenciais = new Merchant(Guid.Parse("d22304f3-81db-43f5-a9da-b68ae96fc20e"), "svbb4w4wo9nPqB7Xx6hYQz9tbjdKFUL2izenJgMM");
                //Merchant credenciais = new Merchant(Guid.Parse("e3c6da09-9a36-4f63-8dbe-28aca4899724"), "73Prs2DVvQNY2SOYI0UdAmKB/6cjxHbfncGXPeCNA2A=");

                _request = new CieloApi(CieloEnvironment.PRODUCTION, credenciais, json);
            }
            else
                _request = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);

            var cb = await _request.ConsultarTransacao(IdPayment);

            return cb;
        }
    }
}
