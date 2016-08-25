using System;
using Ninject.Modules;
using log4net;
using NHibernate;
using Ninject.Web.Common;
using massive_moose.services;
using Ninject;
using massive_moose.storage.azure;
using System.Configuration;

namespace massive_moose.web.owin
{
    public class MassiveMooseNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILog>().ToMethod(x =>
            {
                if (x.Request.Target != null)
                    return LogManager.GetLogger(x.Request.Target.Type);
                return LogManager.GetLogger(typeof(MvcApplication));
            }).InRequestScope();
            Bind<ISessionFactory>().ToMethod(x => SessionFactory.Instance).InSingletonScope();
            Bind<ISession>().ToMethod(x => x.Kernel.Get<ISessionFactory>().OpenSession()).InRequestScope();
            Bind<IFileStorage>().To<AzureFileStorage>();
            Bind<AzureFileStorageConfiguration>().ToMethod(x=>new AzureFileStorageConfiguration {ConnectionString= ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString });
        }
    }
}