﻿using massive_moose.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using massive_moose.web.owin.Models;
using log4net;
using massive_moose.services.models;
using System.Configuration;
using NHibernate.Criterion;

namespace massive_moose.web.owin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog _log;

        public HomeController(ILog log)
        {
            _log = log;
        }

        [Route("/{inviteCode}")]
        public ActionResult Index(string inviteCode=null)
        {
            using (var session = SessionFactory.Instance.OpenStatelessSession())
            {
                Wall wall = null;
                if (string.IsNullOrWhiteSpace(inviteCode))
                {
                    wall = session.CreateCriteria<Wall>()
                        .CreateCriteria("Owner")
                        .Add(Restrictions.Eq("UserName", "system"))
                        .UniqueResult<Wall>();
                }
                else
                {
                    wall = session.CreateCriteria<Wall>()
                        .Add(Restrictions.Eq("InviteCode", inviteCode))
                        .UniqueResult<Wall>();
                }

                var bricks = session.CreateCriteria<Brick>()
                    .Add(Restrictions.Eq("Wall.Id", wall.Id))
                    .List<Brick>();

                var wallVm = new WallViewModel();

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        var b = bricks.SingleOrDefault(br => br.AddressX == j && br.AddressY == i);
                        wallVm.Bricks[i, j] = new BrickViewModel()
                        {
                            AddressX = j,
                            AddressY = i,
                            ImageUrl = (b != null
                                ? string.Format("{0}/v1/image/{1}/{2}",
                                    ConfigurationManager.AppSettings["MMApi"], j, i)
                                : null)
                        };
                    }
                }
                return View(wallVm);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult CreateDatabase()
        {
                SessionFactory.RebuildDatabase();
                return new ContentResult()
                {
                    Content = "Done"
                };
        }
    }
}