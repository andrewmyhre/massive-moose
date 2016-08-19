using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace massive_moose.web.Models
{
    public class BrickViewModel
    {
        public int AddressX { get; set; }
        public int AddressY { get; set; }
        public string ImageUrl { get; set; }
    }

    public class WallViewModel
    {
        public BrickViewModel[,] Bricks { get; set; }

        public WallViewModel()
        {
            Bricks = new BrickViewModel[12,12];
        }
    }
}