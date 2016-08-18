namespace massive_moose.contracts
{
    internal class TextBlock : UIElement
    {
        public TextBlock()
        {
        }

        public Color Color { get; internal set; }
        public string Font { get; internal set; }
        public string FontFamily { get; internal set; }
        public double FontSizeInPixels { get; internal set; }
        public double ForcedHeight { get; internal set; }
        public double ForcedWidth { get; internal set; }
        public string Text { get; internal set; }
        public double V { get; internal set; }
        public double X { get; internal set; }
        public double Y { get; internal set; }
    }
}