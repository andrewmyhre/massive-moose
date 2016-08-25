using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace massive_moose.web.owin
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ManageWall",
                url: "walls/manage/{inviteCode}",
                defaults: new {controller = "Walls", action = "Manage"});

            routes.MapRoute(
                name: "Wall",
                url: "w/{inviteCode}",
                defaults: new { controller = "Home", Action = "Index", inviteCode = UrlParameter.Optional });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }

            );

        }
    }
}
