using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using massive_moose.services.models;
using NHibernate.Criterion;
using NHibernate;
using massive_moose.web.owin.Models;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace massive_moose.web.owin.Controllers
{
    [Authorize]
    public class WallsController : Controller
    {
        private readonly ISession _session;

        public WallsController(ISession session)
        {
            _session = session;
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        // GET: Walls
        public ActionResult Index()
        {
            var owner = _session.CreateCriteria<ApplicationUser>()
                .Add(Restrictions.Eq("Email", User.Identity.Name))
                .UniqueResult<ApplicationUser>();

            if (owner == null)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index");
            }

            var walls = _session.CreateCriteria<Wall>()
                .Add(Restrictions.Eq("Owner.Id", owner.Id))
                .List<Wall>();

            return View(new WallsViewModel {Walls = walls});
        }

        public ActionResult Manage(string inviteCode)
        {
            var wall = _session.CreateCriteria<Wall>()
                .Add(Restrictions.Eq("InviteCode", inviteCode))
                .UniqueResult<Wall>();

            var wallViewModel = new WallViewModel {
                Wall = wall,
                InviteCode =inviteCode
            };

            if (wall.History != null)
                wallViewModel.History = wall.History.OrderBy(h => h.Id).ToArray();

            wallViewModel.Bricks = new BrickViewModel[12,12];
            int originX = 0, originY = 0;

            foreach (var brick in wall.Bricks)
            {
                var relativeX = brick.AddressX + 6;
                var relativeY = brick.AddressY + 6;
                if (relativeX >= 0 && relativeX < 12 && relativeY >= 0 && relativeY < 12)
                {
                    wallViewModel.Bricks[relativeX, relativeY] = new BrickViewModel()
                    {
                        AddressX = brick.AddressX,
                        AddressY = brick.AddressY,
                        ImageUrl = string.Format("{0}/v1/image/t/{1}/{2}/{3}",
                            ConfigurationManager.AppSettings["MMApi"],
                            inviteCode,
                            brick.AddressX,
                            brick.AddressY)
                    };
                }
            }
            if (Request.QueryString["addressX"] != null && Request.QueryString["addressY"] != null)
            {
                int addressX = 0;
                int addressY = 0;
                if (int.TryParse(Request.QueryString["addressX"], out addressX) &&
                    int.TryParse(Request.QueryString["addressY"], out addressY))
                {
                    wallViewModel.BrickHistory = wall.History.Where(x => x.DrawingSession.AddressX == addressX && x.DrawingSession.AddressY == addressY).OrderByDescending(x=>x.Timestamp).ToArray();
                }
            }
            return View(wallViewModel);
        }

        [HttpPost]
        public ActionResult Create()
        {
            var owner = _session.CreateCriteria<ApplicationUser>()
                .Add(Restrictions.Eq("Email", User.Identity.Name))
                .UniqueResult<ApplicationUser>();

            if (owner == null)
                return HttpNotFound();

            var wall = new Wall();
            wall.Owner = owner;
            _session.Save(wall);
            _session.Flush();
            return RedirectToAction("Manage", new {id = wall.InviteCode});
        }

        public ActionResult Share(string id)
        {
            var wall = _session.CreateCriteria<Wall>()
                .Add(Restrictions.Eq("InviteCode", id))
                .UniqueResult<Wall>();

            if (wall == null)
                return HttpNotFound();

            return View("Share", new WallViewModel {Wall = wall, InviteCode = id});
        }
    }
}