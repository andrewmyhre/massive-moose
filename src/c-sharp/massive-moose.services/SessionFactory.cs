using FluentNHibernate.Cfg;
using log4net;
using massive_moose.services.models;
using massive_moose.services.models.mapping;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace massive_moose.services
{
    public class SessionFactory
    {
        private static ILog log = LogManager.GetLogger(typeof(SessionFactory));
        private static ISessionFactory _sessionFactory = null;
        public static ISessionFactory Instance
        {
            get
            {
                if (_sessionFactory == null)
                    _sessionFactory = CreateSessionFactory();
                return _sessionFactory;
            }
        }
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently
                .Configure()
                .Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012
                .ConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings["sql"].ConnectionString))
                .Mappings(x =>
                {
                    x.FluentMappings
                    .AddFromAssemblyOf<BrickMap>();
                })
                .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
                .BuildSessionFactory();
        }

        public static void RebuildDatabase()
        {
            if (_sessionFactory != null) { _sessionFactory.Dispose(); }
            _sessionFactory = Fluently
                .Configure()
                .Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012
                .ConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings["sql"].ConnectionString))
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<BrickMap>())
                .ExposeConfiguration(cfg => new SchemaExport(cfg).Create(false, true))
                .BuildSessionFactory();
        }
    }
}
