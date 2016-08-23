using System.Runtime.Serialization;

namespace massive_moose.contracts.drawing
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "drawingAttributes")]
    public class DrawingAttributes
    {
        [DataMember(Name = "Color")]
        public Color Color { get; set; }
        [DataMember(Name = "OutlineColor")]
        public Color OutlineColor { get; set; }
        [DataMember(Name = "Width")]
        public double Width { get; set; }
        [DataMember(Name = "Height")]
        public double Height { get; set; }

        public DrawingAttributes()
        {
            
        }
    }
}