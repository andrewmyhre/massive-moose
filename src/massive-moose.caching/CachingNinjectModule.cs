using Ninject.Modules;
using System.Configuration;
using Ninject;
using StackExchange.Redis;

namespace massive_moose.caching
{
    public class CachingNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ConnectionMultiplexer>().ToMethod(x =>
                        ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["redis-cache"].ConnectionString))
            .InSingletonScope();
        }
    }
}
