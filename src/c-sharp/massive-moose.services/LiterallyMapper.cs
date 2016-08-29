using massive_moose.services.models.drawing;
using System;
using massive_moose.services.models.literally;
using System.Linq;

namespace massive_moose.services
{
    public class LiterallyMapper
    {
        public Canvas ToCanvas(Drawing drawing)
        {
            var canvas = new Canvas();
            canvas.Width = drawing.ImageSize.Width;
            canvas.Height = drawing.ImageSize.Height;
            canvas.BackgroundColor = new Color(drawing.Colors.Background.A, drawing.Colors.Background.R, drawing.Colors.Background.G, drawing.Colors.Background.B);

            Console.WriteLine("Canvas background color: {0},{1},{2},{3}", canvas.BackgroundColor.R, canvas.BackgroundColor.G, canvas.BackgroundColor.B, canvas.BackgroundColor.A);
            var drawingShapes = drawing.Shapes.OrderBy(s => s.Data.Order);

            InkPresenter inkPresenter = new InkPresenter();
            canvas.Children.Add(inkPresenter);
            foreach (var shape in drawing.Shapes)
            {
                Console.WriteLine("Mapping a: {0}", shape.ClassName);
                switch (shape.ClassName)
                {
                    case "LinePath":
                        MapLinePath(ref canvas, ref inkPresenter, shape);
                        break;
                    case "Line":
                        inkPresenter = null;
                        MapLine(ref canvas, ref inkPresenter, shape);
                        break;
                    case "Rectangle":
                        inkPresenter = null;
                        MapRectangle(ref canvas, shape);
                        break;
                    case "Ellipse":
                        inkPresenter = null;
                        MapEllipse(ref canvas, shape);
                        break;
                    case "Text":
                        inkPresenter = null;
                        MapText(ref canvas, shape);
                        break;
                    case "Polygon":
                        inkPresenter = null;
                        MapPolygon(ref canvas, shape);
                        break;
                    case "ErasedLinePath":
                        MapEraserPath(ref canvas, shape);
                        break;
                    default:
                        inkPresenter = null;
                        break;
                }
            }

            return canvas;
        }

        private void MapPolygon(ref Canvas canvas, Shape shape)
        {
            var polygon = new Polygon();
            polygon.StrokeWidth = shape.Data.StrokeWidth;
            polygon.FillColor = new Color(shape.Data.FillColor.R, shape.Data.FillColor.R, shape.Data.FillColor.G, shape.Data.FillColor.B);
            polygon.StrokeColor = new Color(shape.Data.StrokeColor.A, shape.Data.StrokeColor.R, shape.Data.StrokeColor.G, shape.Data.StrokeColor.B);
            if (shape.Data.Dash != null)
            {
                polygon.Dash = (double[]) shape.Data.Dash;
            }
            polygon.IsClosed = shape.Data.IsClosed;
            polygon.Points = new PointCollection();
            polygon.Points.AddRange(shape.Data.PointCoordinatePairs.Select(p=>new Point(p[0],p[1])));
            canvas.Children.Add(polygon);
        }

        private void MapText(ref Canvas canvas, Shape shape)
        {
            var textBlock = new TextBlock();
            textBlock.Text = shape.Data.Text;
            textBlock.Color = new Color(shape.Data.Color.A, shape.Data.Color.R, shape.Data.Color.G, shape.Data.Color.B);
            textBlock.Font = shape.Data.Font;
            Console.WriteLine("parsing font info: {0}", shape.Data.Font);

            var fontSizeString = shape.Data.Font.Substring(0, shape.Data.Font.IndexOf(" ")+1);
            var fontFamilyString = shape.Data.Font.Substring(shape.Data.Font.IndexOf(" ")+1);

            double fontSize;
            if (double.TryParse(fontSizeString.Replace("px",""), out fontSize))
            {
                Console.WriteLine("Font size in pixels:{0}", fontSize);
                textBlock.FontSizeInPixels = fontSize;
            }

            Console.WriteLine("Font Family:{0}", fontFamilyString);
            textBlock.FontFamily = fontFamilyString;

            textBlock.ForcedWidth = shape.Data.ForcedWidth;
            textBlock.ForcedHeight = shape.Data.ForcedHeight;
            textBlock.V = shape.Data.V;
            textBlock.X = shape.Data.X;
            textBlock.Y = shape.Data.Y;
            canvas.Children.Add(textBlock);
        }

        private void MapEllipse(ref Canvas canvas, Shape shape)
        {
            var ellipse = new Ellipse();
            ellipse.X = shape.Data.X;
            ellipse.Y = shape.Data.Y;
            ellipse.Width = shape.Data.Width;
            ellipse.Height = shape.Data.Height;
            ellipse.StrokeWidth = shape.Data.StrokeWidth;
            ellipse.StrokeColor = new Color(shape.Data.StrokeColor.A, shape.Data.StrokeColor.R, shape.Data.StrokeColor.G, shape.Data.StrokeColor.B);
            ellipse.FillColor = new Color(shape.Data.FillColor.A, shape.Data.FillColor.R, shape.Data.FillColor.G, shape.Data.FillColor.B);
            canvas.Children.Add(ellipse);
        }

        private void MapRectangle(ref Canvas canvas, Shape shape)
        {
            var rect = new Rectangle();
            rect.X = shape.Data.X;
            rect.Y = shape.Data.Y;
            rect.Width = shape.Data.Width;
            rect.Height = shape.Data.Height;
            rect.StrokeWidth = shape.Data.StrokeWidth;
            rect.StrokeColor = new Color(shape.Data.StrokeColor.A, shape.Data.StrokeColor.R, shape.Data.StrokeColor.G, shape.Data.StrokeColor.B);
            rect.FillColor = new Color(shape.Data.FillColor.A, shape.Data.FillColor.R, shape.Data.FillColor.G, shape.Data.FillColor.B);
            canvas.Children.Add(rect);
        }

        private void MapLine(ref Canvas canvas, ref InkPresenter inkPresenter, Shape shape)
        {
            var line = new Line();
            line.X1 = shape.Data.X1;
            line.Y1 = shape.Data.Y1;
            line.X2 = shape.Data.X2;
            line.Y2 = shape.Data.Y2;
            if (shape.Data.Dash != null)
                line.Dash = shape.Data.Dash.Select(d=>d/shape.Data.StrokeWidth).ToArray();
            line.Brush = new SolidColorBrush(new Color(shape.Data.Color.A, shape.Data.Color.R, shape.Data.Color.G, shape.Data.Color.B));
            line.LineThickness = shape.Data.StrokeWidth;
            canvas.Children.Add(line);
        }

        private void MapLinePath(ref Canvas canvas, ref InkPresenter inkPresenter, Shape shape)
        {
            if (inkPresenter == null)
            {
                inkPresenter = new InkPresenter();
                canvas.Children.Add(inkPresenter);
            }

            if (shape.Data.SmoothedPointCoordinatePairs != null)
            {
                var stroke = new Stroke();
                stroke.DrawingAttributes.Width = stroke.DrawingAttributes.Height = shape.Data.PointSize;
                stroke.DrawingAttributes.Color = new Color(shape.Data.PointColor.A, shape.Data.PointColor.R, shape.Data.PointColor.G, shape.Data.PointColor.B);
                stroke.StylusPoints.AddRange(shape.Data.SmoothedPointCoordinatePairs.Select(p => new StylusPoint() { X = p[0], Y = p[1], PressureFactor = 1.0f }));
                inkPresenter.Strokes.Add(stroke);
            }
            else if (shape.Data.PointCoordinatePairs != null)
            {
                var stroke = new Stroke();
                stroke.DrawingAttributes.Color = new Color(shape.Data.PointColor.A, shape.Data.PointColor.R, shape.Data.PointColor.G, shape.Data.PointColor.B);
                stroke.DrawingAttributes.Width = stroke.DrawingAttributes.Height = shape.Data.PointSize;
                stroke.StylusPoints.AddRange(shape.Data.PointCoordinatePairs.Select(p=>new StylusPoint() {X=p[0],Y=p[1], PressureFactor = 1.0f}));
                inkPresenter.Strokes.Add(stroke);
            }
            
        }
        private void MapEraserPath(ref Canvas canvas, Shape shape)
        {
            var eraser = new Eraser();

            if (shape.Data.SmoothedPointCoordinatePairs != null)
            {
                var stroke = new Stroke();
                stroke.DrawingAttributes.Width = stroke.DrawingAttributes.Height = shape.Data.PointSize;
                stroke.StylusPoints.AddRange(shape.Data.SmoothedPointCoordinatePairs.Select(p => new StylusPoint() { X = p[0], Y = p[1], PressureFactor = 1.0f }));
                eraser.Strokes.Add(stroke);
            }
            else if (shape.Data.PointCoordinatePairs != null)
            {
                var stroke = new Stroke();
                stroke.DrawingAttributes.Width = stroke.DrawingAttributes.Height = shape.Data.PointSize;
                stroke.StylusPoints.AddRange(shape.Data.PointCoordinatePairs.Select(p => new StylusPoint() { X = p[0], Y = p[1], PressureFactor = 1.0f }));
                eraser.Strokes.Add(stroke);
            }
            eraser.PointSize = shape.Data.PointSize;
            canvas.Children.Add(eraser);

        }

        byte[] HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = hue2rgb(p, q, h + 1 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3);
            }

            return new byte[] {(byte)Math.Round(r * 255,0), (byte)Math.Round(g * 255,0), (byte)Math.Round(b * 255,0)};
        }

        double hue2rgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        }
    }
}
