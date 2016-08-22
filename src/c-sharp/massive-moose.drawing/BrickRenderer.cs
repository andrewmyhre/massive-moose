using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using massive_moose.contracts;

namespace massive_moose.drawing
{
    public class BrickRenderer
    {
        private System.Windows.Media.Color brick = System.Windows.Media.Color.FromRgb(237, 116, 40);
        public byte[] Render(Canvas canvasContract, Stream imageSource=null)
        {
            byte[] result = null;
            System.Threading.Thread t = new Thread(()=>
            {

                System.Windows.Controls.Grid grid = new System.Windows.Controls.Grid();
                grid.ClipToBounds = true;
                grid.Background =
                grid.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(canvasContract.BackgroundColor.A, canvasContract.BackgroundColor.R, canvasContract.BackgroundColor.G, canvasContract.BackgroundColor.B));
                System.Windows.Controls.Canvas canvas = new System.Windows.Controls.Canvas();

                if (imageSource != null)
                {
                    System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = imageSource;
                    bmp.EndInit();
                    var image = new System.Windows.Controls.Image();
                    image.Source = bmp;
                    image.Stretch = Stretch.Fill;
                    grid.Children.Add(image);
                }

                foreach (var child in canvasContract.Children)
                {
                    Console.WriteLine("Drawing an object: {0}", child);
                    if (child is InkPresenter)
                    {
                        var input = child as InkPresenter;
                        var inkPresenter = new System.Windows.Controls.InkPresenter();
                        var drawingAttributes = new System.Windows.Ink.DrawingAttributes();

                        foreach (var stroke in input.Strokes)
                        {
                            inkPresenter.Strokes.Add(
                                new System.Windows.Ink.Stroke(
                                    new System.Windows.Input.StylusPointCollection(
                                        stroke.StylusPoints.Select(sp => new System.Windows.Input.StylusPoint() { X = sp.X, Y = sp.Y, PressureFactor = sp.PressureFactor})),
                                    new System.Windows.Ink.DrawingAttributes()
                                    {
                                        Color = System.Windows.Media.Color.FromArgb(stroke.DrawingAttributes.Color.A, stroke.DrawingAttributes.Color.R, stroke.DrawingAttributes.Color.G, stroke.DrawingAttributes.Color.B),
                                        Width = stroke.DrawingAttributes.Width,
                                        Height = stroke.DrawingAttributes.Height,
                                        FitToCurve = true,
                                        StylusTip = StylusTip.Ellipse
                                    }));
                        }

                        canvas.Children.Add(inkPresenter);
                    }else if (child is Line)
                    {
                        var input = child as Line;
                        var brush = input.Brush as massive_moose.contracts.SolidColorBrush;
                        var line = new System.Windows.Shapes.Line();
                        line.X1 = input.X1;
                        line.Y1 = input.Y1;
                        line.X2 = input.X2;
                        line.Y2 = input.Y2;
                        line.Stroke = new System.Windows.Media.SolidColorBrush(new System.Windows.Media.Color
                        {
                            A=brush.Color.A,R=brush.Color.R,G=brush.Color.G,B=brush.Color.B
                        });
                        if (input.Dash != null)
                        {
                            line.StrokeDashArray = new System.Windows.Media.DoubleCollection(input.Dash);
                        }
                        line.StrokeThickness = input.LineThickness;
                        canvas.Children.Add(line);
                    }
                    else if (child is Polyline)
                    {
                        var input = child as Polyline;
                        var brush = input.Brush as massive_moose.contracts.SolidColorBrush;
                        var drawingAttributes = new System.Windows.Ink.DrawingAttributes()
                        {
                            Color = System.Windows.Media.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B),
                            Width = input.StrokeThickness,
                            Height = input.StrokeThickness,
                            FitToCurve = true,
                            StylusTip = StylusTip.Ellipse
                        };

                        var polyline = new System.Windows.Shapes.Polyline();
                        foreach (var p in input.Points)
                        {
                            polyline.Points.Add(new System.Windows.Point(p.X, p.Y));
                        }
                    } else if (child is Rectangle)
                    {
                        var input = child as Rectangle;
                        var shape = new System.Windows.Shapes.Rectangle();
                        shape.Width = input.Width;
                        shape.Height = input.Height;
                        shape.Stroke=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.StrokeColor.A, input.StrokeColor.R, input.StrokeColor.G, input.StrokeColor.B));
                        shape.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.FillColor.A, input.FillColor.R, input.FillColor.G, input.FillColor.B));
                        shape.StrokeThickness = input.StrokeWidth;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, input.X);
                        System.Windows.Controls.Canvas.SetTop(shape, input.Y);
                    } else if (child is Polygon)
                    {
                        var input = child as Polygon;
                        var shape = new System.Windows.Shapes.Polygon();
                        shape.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.StrokeColor.A, input.StrokeColor.R, input.StrokeColor.G, input.StrokeColor.B));
                        shape.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.FillColor.A, input.FillColor.R, input.FillColor.G, input.FillColor.B));
                        shape.StrokeThickness = input.StrokeWidth;
                        foreach (var p in input.Points)
                        {
                            shape.Points.Add(new System.Windows.Point(p.X, p.Y));
                        }

                        canvas.Children.Add(shape);
                    }
                    else if (child is Ellipse)
                    {
                        var input = child as Ellipse;
                        var shape = new System.Windows.Shapes.Rectangle();
                        shape.Width = input.Width;
                        shape.Height = input.Height;
                        shape.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.StrokeColor.A, input.StrokeColor.R, input.StrokeColor.G, input.StrokeColor.B));
                        shape.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.FillColor.A, input.FillColor.R, input.FillColor.G, input.FillColor.B));
                        shape.StrokeThickness = input.StrokeWidth;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, input.X);
                        System.Windows.Controls.Canvas.SetTop(shape, input.Y);
                    } else if (child is TextBlock)
                    {
                        var input = child as TextBlock;
                        var shape = new System.Windows.Controls.TextBlock();
                        shape.Text = input.Text;
                        if (!string.IsNullOrWhiteSpace(input.FontFamily))
                            shape.FontFamily = new FontFamily(input.FontFamily);
                        if (input.FontSizeInPixels != 0)
                        {
                            shape.FontSize = input.FontSizeInPixels * 72 / 96;

                        
                        }
                        shape.MaxWidth = input.ForcedWidth;
                        //shape.MaxHeight = input.ForcedHeight;
                        shape.TextWrapping = TextWrapping.Wrap;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, input.X);
                        System.Windows.Controls.Canvas.SetTop(shape, input.Y);
                    }
                }

                System.Windows.Media.Transform transform = canvas.LayoutTransform;
                // reset current transform (in case it is scaled or rotated)
                canvas.LayoutTransform = null;
                grid.Children.Add(canvas);



                grid.Width = 1600;
                grid.Height = 900;

                // Get the size of canvas
                System.Windows.Size size = new System.Windows.Size(grid.Width, grid.Height);

                // Measure and arrange the surface
                // VERY IMPORTANT
                grid.Measure(size);
                grid.Arrange(new System.Windows.Rect(size));
                grid.Background =
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0));

                using (var outputStream = new MemoryStream())
                {
                    SaveCanvas(grid, 96, outputStream);
                    result = outputStream.ToArray();
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            return result;
        }

        public static void SaveCanvas(System.Windows.UIElement canvas, int dpi, Stream outputStream)
        {
            System.Windows.Size size = new System.Windows.Size(1600, 900);
            canvas.Measure(size);
            //canvas.Arrange(new Rect(size));

            var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(
                1600, //width 
                900, //height 
                dpi, //dpi x 
                dpi, //dpi y 
                System.Windows.Media.PixelFormats.Pbgra32 // pixelformat 
                );
            rtb.Render(canvas);

            SaveRTBAsPNG(rtb, outputStream);
        }

        private static void SaveRTBAsPNG(System.Windows.Media.Imaging.RenderTargetBitmap bmp, Stream outputStream)
        {
            var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
            enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

            enc.Save(outputStream);
        }
    }
}
