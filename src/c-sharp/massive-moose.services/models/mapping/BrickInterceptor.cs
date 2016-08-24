using log4net;
using NHibernate;
using NHibernate.Type;
using System;

namespace massive_moose.services.models.mapping
{
    public class BrickInterceptor : EmptyInterceptor
    {
        private static ILog log = log4net.LogManager.GetLogger(typeof(BrickInterceptor));
        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
            var brick = entity as Brick;
            if (brick != null)
            {
                brick.LastUpdated = DateTime.Now;
                log.DebugFormat("Update timestamp for brick {0}", brick.Id);
                return true;
            }
            return false;
        }

        public override void OnCollectionUpdate(object collection, object key)
        {
            base.OnCollectionUpdate(collection, key);
        }
    }
}