namespace massive_moose.contracts.drawing
{
    public class Ellipse : UIElement
    {
        public Ellipse()
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