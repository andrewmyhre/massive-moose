using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.services.models.drawing
{
    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "stroke")]
    public class Stroke
    {
        [DataMember(Name ="DrawingAttributes")]
        public DrawingAttributes DrawingAttributes { get; set; }
        [DataMember(Name = "StylusPoints")]
        public StylusPointCollection StylusPoints { get; set; }

        public Stroke(IEnumerable<StylusPoint> stylusPoints=null)
        {
            DrawingAttributes = new DrawingAttributes();
            StylusPoints = new StylusPointCollection();
            if (stylusPoints != null)
                StylusPoints.AddRange(stylusPoints);
        }

        public Stroke(StylusPointCollection stylusPointCollection)
        {
            DrawingAttributes = new DrawingAttributes();
            StylusPoints = stylusPointCollection;
        }
    }
}