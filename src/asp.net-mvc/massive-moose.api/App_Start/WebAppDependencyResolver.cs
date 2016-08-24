using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace massive_moose.api
{
    public class WebAppDependencyResolver : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            return massive_moose.api.App_Start.NinjectWebCommon.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return massive_moose.api.App_Start.NinjectWebCommon.GetServices(serviceType);
        }
    }
}