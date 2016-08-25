using massive_moose.services.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace massive_moose.web.owin.Models
{
    public class WallsViewModel
    {
        public IList<Wall> Walls { get; set; }
    }
}