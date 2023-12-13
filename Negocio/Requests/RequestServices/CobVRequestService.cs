﻿using Negocio.Models;
using Negocio.Requests.RequestModels;
using Negocio.Requests.RequestServices.Base;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Requests.RequestServices
{
    /// <summary>
    /// Reúne endpoints destinados a lidar com gerenciamento de cobranças com vencimento.
    /// </summary>
    public class CobVRequestService : RequestBase
    {
        public CobVRequestService()
        {
            SetRoute("cobv");
        }

        /// <summary>
        /// Criar cobrança imediata usando um identificador
        /// </summary>
        /// <param name="txId"></param>
        /// <param name="cob"></param>
        /// <returns></returns>
        public async Task<Cob> Create(string txId, CobVRequest cob)
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
    }
}
