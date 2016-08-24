using System.Linq;

namespace massive_moose.client.silverlight
{
    public class GeometryToContractMapper
    {
        public Canvas Map(System.Windows.Controls.Canvas source)
        {
            var canvas = new Canvas();
            foreach (var child in source.Children)
            {
                if (child is System.Windows.Shapes.Polyline)
                {
                    var sourceChild = child as System.Windows.Shapes.Polyline;
                    var pl = new Polyline();
                    pl.Points.AddRange(
                        ((System.Windows.Shapes.Polyline) child).Points.Select(p => new Point(p.X, p.Y)));
                    pl.StrokeThickness = sourceChild.StrokeThickness;

                    var solidColor = sourceChild.Stroke as System.Windows.Media.SolidColorBrush;
                    pl.Brush =
                        new SolidColorBrush(new Color(solidColor.Color.A, solidColor.Color.R, solidColor.Color.G,
                            solidColor.Color.B));
                    canvas.Children.Add(pl);
                }
            }

            return canvas;
        }

        public Canvas Map(System.Windows.Controls.InkPresenter inkPresenter)
        {
            var canvas = new Canvas();
            var source = inkPresenter as System.Windows.Controls.InkPresenter;
            var ip = new InkPresenter();
            ip.Strokes.AddRange(source.Strokes.Select(s => new Stroke()
            {
                StylusPoints =
                    new StylusPointCollection(
                        s.StylusPoints.Select(
                            sp => new StylusPoint() { X = sp.X, Y = sp.Y, PressureFactor = sp.PressureFactor })),
                DrawingAttributes = new DrawingAttributes()
                {
                    Color =
                        new Color(s.DrawingAttributes.Color.A, s.DrawingAttributes.Color.R,
                            s.DrawingAttributes.Color.G, s.DrawingAttributes.Color.B),
                    Width = s.DrawingAttributes.Width,
                    Height = s.DrawingAttributes.Height,
                    OutlineColor =
                        new Color(s.DrawingAttributes.OutlineColor.A, s.DrawingAttributes.OutlineColor.R,
                            s.DrawingAttributes.OutlineColor.G, s.DrawingAttributes.OutlineColor.B)
                }
            }));
            canvas.Children.Add(ip);
            return canvas;
        }
    }
}
