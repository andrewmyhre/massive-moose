namespace massive_moose.contracts.drawing
{
    public class TextBlock : UIElement
    {
        public TextBlock()
        {
        }

        public Color Color { get; set; }
        public string Font { get; set; }
        public string FontFamily { get; set; }
        public double FontSizeInPixels { get; set; }
        public double ForcedHeight { get; set; }
        public double ForcedWidth { get; set; }
        public string Text { get; set; }
        public double V { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}