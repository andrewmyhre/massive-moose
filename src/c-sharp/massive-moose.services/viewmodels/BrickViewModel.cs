using System.Configuration;

namespace massive_moose.services.viewmodels
{
    public class BrickWallSet
    {
        public BrickViewModel[,] Set { get; private set; }

        public BrickWallSet()
        {

        }

        public BrickWallSet(BrickViewModel[,] set)
        {
            Set = set;
        }
    }
    public class BrickViewModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int C { get; set; }
        public int U { get; internal set; }
        public long D { get; internal set; }
    }

}