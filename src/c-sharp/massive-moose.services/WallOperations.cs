using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using massive_moose.services.models;
using NHibernate;
using NHibernate.Criterion;

namespace massive_moose.services
{
    public class WallOperations
    {
        public Wall GetWallByKeyOrDefault(string wallKey, IStatelessSession session)
        {
            if (!string.IsNullOrWhiteSpace(wallKey))
            {
                return session.CreateCriteria<Wall>()
                    .Add(Restrictions.Eq("InviteCode", wallKey))
                    .UniqueResult<Wall>();
            }
            else
            {
                return session.Get<Wall>(1);
            }
        }
        public Wall GetWallByKeyOrDefault(string wallKey, ISession session)
        {
            if (!string.IsNullOrWhiteSpace(wallKey))
            {
                return session.CreateCriteria<Wall>()
                    .Add(Restrictions.Eq("InviteCode", wallKey))
                    .UniqueResult<Wall>();
            }
            else
            {
                return session.Get<Wall>(1);
            }
        }
    }
}
