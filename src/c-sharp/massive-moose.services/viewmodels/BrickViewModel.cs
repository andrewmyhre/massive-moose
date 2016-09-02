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
    [DataContract(Name ="brick")]
    public class BrickViewModel
    {
        [DataMember(Name ="x")]
        public int X { get; set; }
        [DataMember(Name = "y")]
        public int Y { get; set; }
        [DataMember(Name = "c")]
        public int C { get; set; }
        [DataMember(Name = "u")]
        public int U { get; internal set; }
        [DataMember(Name = "d")]
        public long D { get; internal set; }
    }

}