namespace massive_moose.contracts.drawing
{
    public class Polygon : UIElement
    {
        public Polygon()
        {
        }

        public double[] Dash { get; set; }
        public Color FillColor { get; set; }
        public bool IsClosed { get; set; }
        public PointCollection Points { get; set; }
        public Color StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
    }
}