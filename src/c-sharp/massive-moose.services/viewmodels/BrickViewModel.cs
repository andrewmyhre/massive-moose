using System.Configuration;
using System.Runtime.Serialization;

namespace massive_moose.services.viewmodels
{
    [DataContract(Name ="brickWallSet")]
    public class BrickWallSet
    {
        [DataMember(Name="set")]
        public BrickViewModel[,] Set { get; private set; }
        [DataMember(Name = "hello")]
        public string Hello { get; set; }

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