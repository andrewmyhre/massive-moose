using System.Runtime.Serialization;
namespace massive_moose.contracts
{
    [DataContract(Namespace= "http://www.massivemoose.com/2016/xmlschema/contracts", Name="line")]
    [KnownType(typeof(SolidColorBrush))]
    public class Line : UIElement
    {
        [DataMember(Name="x1")]
        public double X1 { get; set; }
        [DataMember(Name = "y1")]
        public double Y1 { get; set; }
        [DataMember(Name = "x2")]
        public double X2 { get; set; }
        [DataMember(Name = "y2")]
        public double Y2 { get; set; }

        [DataMember(Name = "lineThickness")]
        public double LineThickness { get; set; }
        [DataMember(Name = "brush")]
        public Brush Brush { get; set; }
        [DataMember(Name="dash")]
        public double[] Dash { get; set; }
    }

    [DataContract(Namespace = "http://www.massivemoose.com/2016/xmlschema/contracts", Name = "polyline")]
    [KnownType(typeof(SolidColorBrush))]
    public class Polyline : UIElement
    {
        [DataMember(Name = "Points")]
        public PointCollection Points { get; private set; }
        [DataMember(Name = "StrokeThickness")]
        public double StrokeThickness { get; set; }
        [DataMember(Name = "Brush")]
        public Brush Brush { get; set; }

        public Polyline()
        {
            Points = new PointCollection();
        }
    }
}