using System.Web.Http;
using Microsoft.Owin.Security.OAuth;
using WebApiContrib.IoC.Ninject;
using massive_moose.api.App_Start;

namespace massive_moose.api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            
            config.DependencyResolver = new NinjectResolver(NinjectWebCommon.CreateKernel());
            
        }
    }
}
