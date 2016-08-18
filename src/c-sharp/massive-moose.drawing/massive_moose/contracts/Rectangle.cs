namespace massive_moose.contracts
{
    internal class Rectangle : UIElement
    {
        public Rectangle()
        {
        }

        public Color FillColor { get; internal set; }
        public double Height { get; internal set; }
        public Color StrokeColor { get; internal set; }
        public double StrokeWidth { get; internal set; }
        public double Width { get; internal set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }
    }
}