using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NegocioCielo.Models
{
    public class EntradaPagamentoCielo
    {
        public virtual int id_pagamento { get; set; }
        public virtual DadosCartao dadoscartao { get; set; }
        public virtual DadosCobranca dadoscobranca { get; set; }
    }

    public class DadosCartao
    {
        public virtual string nome_cartao { get; set; }

        public virtual string numero_cartao { get; set; }

        public virtual string validade_cartao { get; set; }

        public virtual string cod_seguranca { get; set; }

        public virtual string bandeira_cartao { get; set; }
    }

    public class DadosCobranca
    {
        public virtual decimal valor { get; set; }
        public virtual int parcelas { get; set; }
        public virtual string descricaoBreve { get; set; }        

    }

}
