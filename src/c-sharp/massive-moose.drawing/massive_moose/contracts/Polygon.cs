namespace massive_moose.contracts
{
    internal class Polygon : UIElement
    {
        public Polygon()
        {
        }

        public double[] Dash { get; internal set; }
        public Color FillColor { get; internal set; }
        public bool IsClosed { get; internal set; }
        public PointCollection Points { get; internal set; }
        public Color StrokeColor { get; internal set; }
        public double StrokeWidth { get; internal set; }
    }
}