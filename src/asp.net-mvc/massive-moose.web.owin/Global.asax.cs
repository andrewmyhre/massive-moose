using log4net;
using massive_moose.services;
using NHibernate;
using Ninject;
using Ninject.Web.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace massive_moose.web.owin
{
    public class MvcApplication : NinjectHttpApplication
    {
        private static IKernel _kernel = null;
        public static IKernel GetKernel()
        {
            if (_kernel != null) return _kernel;

            _kernel = new StandardKernel();
            try
            {
                _kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                _kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                _kernel.Load(new MassiveMooseNinjectModule());
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
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected override IKernel CreateKernel()
        {
            return GetKernel();
        }
    }
}
