using System.Configuration;

namespace massive_moose.services.viewmodels
{
    public class BrickViewModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int C { get; set; }
        public int U { get; internal set; }
        public long D { get; internal set; }
    }

}