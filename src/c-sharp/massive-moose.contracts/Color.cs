using System.Runtime.Serialization;

namespace massive_moose.contracts
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "color")]
    public class Color
    {
        [DataMember(Name="A")]
        public byte A { get; private set; }
        [DataMember(Name = "R")]
        public byte R { get; private set; }
        [DataMember(Name = "G")]
        public byte G { get; private set; }
        [DataMember(Name = "B")]
        public byte B { get; private set; }

        public Color(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
    }
}