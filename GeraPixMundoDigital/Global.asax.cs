using Negocio;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace GeraPixMundoDigital
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var config = GlobalConfiguration.Configuration;
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
      
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            
            
                /*new StartConfig(
                _baseUrl: ConfigurationManager.AppSettings["url"].ToString(),
                _clientId: ConfigurationManager.AppSettings["cliente"].ToString(),
                _clientSecret: ConfigurationManager.AppSettings["senha"].ToString(),
                _certificate: new System.Security.Cryptography.X509Certificates.X509Certificate2(ConfigurationManager.AppSettings["arquivo_chave"].ToString(), ConfigurationManager.AppSettings["senha_arquivo"].ToString()));*/
            
            
        }
    }
}
