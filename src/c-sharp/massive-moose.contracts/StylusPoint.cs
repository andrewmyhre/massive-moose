using System.Runtime.Serialization;
namespace massive_moose.contracts
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "stylusPoint")]
    public class StylusPoint
    {
        [DataMember(Name = "X")]
        public double X { get; set; }
        [DataMember(Name = "Y")]
        public double Y { get; set; }
        [DataMember(Name = "PressureFactor")]
        public float PressureFactor { get; set; }
    }
}