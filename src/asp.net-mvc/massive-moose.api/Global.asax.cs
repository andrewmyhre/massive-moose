using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using log4net;
using massive_moose.services;
using Ninject;
using Ninject.Web.Common;
using massive_moose.caching;

namespace massive_moose.api
{
    public class WebApiApplication : NinjectHttpApplication
    {
        private static ILog Log = LogManager.GetLogger(typeof(WebApiApplication));
        private static IKernel _kernel = null;
        public static IKernel GetKernel()
        {
            if (_kernel != null) return _kernel;

            _kernel = new StandardKernel();
            try
            {
                _kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                _kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                _kernel.Load(new NinjectModule());
                _kernel.Load(new MassiveMooseNinjectModule());
                _kernel.Load(new CachingNinjectModule());
                return _kernel;
            }
            catch
            {
                _kernel.Dispose();
                throw;
            }
        }

        protected override void OnApplicationStarted()
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnApplicationStarted();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception != null)
            {
                Log.Error("Unhandled exception", exception);
            }

            HttpException httpException = exception as HttpException;
            if (httpException != null)
            {
                RouteData routeData = new RouteData();
                routeData.Values.Add("controller", "Error");
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // page not found
                        //routeData.Values.Add("action", "HttpError404");
                        break;
                    case 500:
                        // server error
                        //routeData.Values.Add("action", "HttpError500");
                        break;
                    default:
                        routeData.Values.Add("action", "Index");
                        break;
                }
                routeData.Values.Add("error", exception.Message);
                // clear error on server
                Server.ClearError();

                //Response.RedirectToRoute(routeData.Values);
            }
            Server.ClearError();
        }

        protected override IKernel CreateKernel()
        {
            return GetKernel();
        }
    }
}
