﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using massive_moose.caching;
using massive_moose.services;
using massive_moose.services.models;
using massive_moose.services.viewmodels;
using massive_moose.storage.azure;
using NHibernate;
using Ninject;
using Ninject.Web.Common;
using massive_moose.services.caching;

namespace massive_moose.api
{
    public class NinjectModule : Ninject.Modules.NinjectModule
    {
        public override void Load()
        {
            Bind<IFileStorage>().To<AzureFileStorage>();
            Bind<IObjectCache<Wall>>().To<RedisCache<Wall>>().InRequestScope();
            Bind<IObjectCache<BrickWallSet>>().To<RedisCache<BrickWallSet>>().InRequestScope();
            Bind<AzureFileStorageConfiguration>().ToMethod(x => new AzureFileStorageConfiguration { ConnectionString = ConfigurationManager.ConnectionStrings["azure-storage"].ConnectionString });
            Bind<ISession>().ToMethod(x => x.Kernel.Get<ISessionFactory>().OpenSession()).InRequestScope();
        }
    }
}