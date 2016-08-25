﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using log4net;
using massive_moose.services.models;
using massive_moose.services;
using NHibernate.Criterion;
using massive_moose.api.Models;

namespace massive_moose.api.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class V2Controller : ApiController
    {
        private static ILog Log = LogManager.GetLogger(typeof(V2Controller));
        [HttpGet]
        [Route("v2/wall/{wallKey}/{originX}/{originY}")]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public BrickViewModel[,] Wall(int originX, int originY, string wallKey = null)
        {
            using (var session = SessionFactory.Instance.OpenSession())
            {
                Wall wallRecord = new WallOperations().GetWallByKeyOrDefault(wallKey, session);

                IList<object[]> bricks = session.QueryOver<Brick>()
                    .And(b => b.Wall.Id == wallRecord.Id)
                    .Select(
                        b => b.AddressX,
                        b => b.AddressY,
                        b => b.LastUpdated,
                        b => b.Guid).List<object[]>();

                var wall = new BrickViewModel[12, 12];
                for (int y = 0; y < 12; y++)
                {
                    for (int x = 0; x < 12; x++)
                    {
                        var relativeX = originX - 6 + x;
                        var relativeY = originY - 6 + y;
                        var o= bricks.SingleOrDefault(b => (int)b[0] == relativeX && (int)b[1] == relativeY);
                        if (o != null)
                        {
                            wall[x, y] = new BrickViewModel()
                            {
                                X = (int) o[0],
                                Y = (int) o[1],
                                D = ((DateTime?)o[2]).HasValue?((DateTime?)o[2]).Value.Ticks.ToString():"",
                                G = o[3].ToString()
                            };
                        }
                        else
                        {
                            wall[x, y] = new BrickViewModel()
                            {
                                X = relativeX,
                                Y = relativeY,
                                G = ""
                            };
                        }
                    }
                }

                return wall;
            }
        }
    }
}