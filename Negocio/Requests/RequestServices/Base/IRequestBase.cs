﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Negocio.Requests.RequestServices.Base
{
    public interface IRequestBase : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        //string GetUrlRequest();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(object data);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(object data, Dictionary<string, string> headers);
    }
}
