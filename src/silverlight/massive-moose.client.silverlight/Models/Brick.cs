using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace massive_moose.client.silverlight.Models
{
    public class Brick
    {
        public Brick()
        {
            Info = "_";
        }
        public int AddressX { get; set; }
        public int AddressY { get; set; }
        public Guid Guid { get; set; }
        public int Id { get; set; }
        public string AddressLabel { get { return string.Concat(AddressX.ToString(), ":", AddressY.ToString()); } }
        public string Info { get; set; }
        public int ViewSpaceX { get; internal set; }
        public int ViewSpaceY { get; internal set; }
    }
}
