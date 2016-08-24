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

                RegisterServices(_kernel);
                return _kernel;
            }
            catch
            {
                _kernel.Dispose();
                throw;
            }
        }

        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<ILog>().ToMethod(x =>
            {
                if (x.Request.Target != null)
                    return LogManager.GetLogger(x.Request.Target.Type);
                return LogManager.GetLogger(typeof(MvcApplication));
            }).InRequestScope();
            kernel.Bind<ISessionFactory>().ToMethod(x => SessionFactory.Instance).InSingletonScope();
            kernel.Bind<ISession>().ToMethod(x => x.Kernel.Get<ISessionFactory>().OpenSession()).InRequestScope();
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
