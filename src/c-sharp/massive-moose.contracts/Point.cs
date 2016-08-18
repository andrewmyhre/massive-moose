using System.Runtime.Serialization;
namespace massive_moose.contracts
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "point")]
    public class Point
    {
        [DataMember(Name = "X")]
        public double X { get; private set; }
        [DataMember(Name = "Y")]
        public double Y { get; private set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}