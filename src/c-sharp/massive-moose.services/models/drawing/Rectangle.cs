using System.Collections.Generic;
using System.Runtime.Serialization;

namespace massive_moose.services.models.drawing
{
    public class Rectangle : UIElement
    {
        public Rectangle()
        {
        }

        public Color FillColor { get; set; }
        public double Height { get; set; }
        public Color StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
        public double Width { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}