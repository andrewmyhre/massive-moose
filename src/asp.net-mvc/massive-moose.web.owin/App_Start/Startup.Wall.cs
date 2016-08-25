using System.Collections.Generic;
using Microsoft.Owin;
using Owin;
using massive_moose.services;
using massive_moose.services.models;
using NHibernate.Criterion;
using System;

namespace massive_moose.web.owin
{
	public partial class Startup
	{
	    private static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Startup));
	    public void CreateDefaultWall(IAppBuilder app)
	    {
            using (var session = massive_moose.services.SessionFactory.Instance.OpenSession())
			using (var tx = session.BeginTransaction())
			{
			    bool creatingObjects = false;
                var systemUser = session.CreateCriteria<ApplicationUser>().Add(Restrictions.Eq("UserName","system")).UniqueResult<ApplicationUser>();
                if (systemUser == null)
                {
                    systemUser = new ApplicationUser()
                    {
                        Email = "system@massivemoose.com",
                        EmailConfirmed = true,
                        UserName = "system",
                        PasswordHash = "1234"
                    };
                    Log.InfoFormat("Creating a system user");
                    session.Save(systemUser);
                    creatingObjects = true;
                }

                var walls = session.CreateCriteria<Wall>().SetProjection(Projections.Count("Id")).UniqueResult<int>();
                if (walls == 0)
                {
                    var wall = new Wall()
                    {
                        Owner = systemUser
                    };
                    Log.InfoFormat("Creating a default wall");
                    session.Save(wall);
                    creatingObjects = true;
                }

			    if (creatingObjects)
			    {
			        tx.Commit();
			    }
			}
	    }

    }
}