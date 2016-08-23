using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.contracts.drawing
{
    [DataContract(Namespace= "http://www.massivemoose.com/2016/xmlschema/contracts", Name="canvas")]
    [KnownType(typeof(Polyline))]
    [KnownType(typeof(InkPresenter))]
    public class Canvas
    {
        [DataMember(Name="children")]
        public List<UIElement> Children { get; private set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color BackgroundColor { get; set; }

        public Canvas()
        {
            Children = new List<UIElement>();
        }
    }
}