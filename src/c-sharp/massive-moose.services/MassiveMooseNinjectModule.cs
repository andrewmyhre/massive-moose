using System;
using System.Configuration;
using log4net;
using Ninject.Modules;
using NHibernate;
using Ninject;

namespace massive_moose.services
{
    public class MassiveMooseNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILog>().ToMethod(x =>
            {
                if (x.Request.Target != null)
                    return LogManager.GetLogger((Type) x.Request.Target.Type);
                return LogManager.GetLogger("massive_moose");
            });
            Bind<ISessionFactory>().ToMethod(x => SessionFactory.Instance).InSingletonScope();
            Bind<IWallOperations>().To<WallOperations>();
        }
    }
}