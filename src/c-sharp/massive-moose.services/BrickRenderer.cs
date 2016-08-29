using massive_moose.services.models.drawing;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Ink;
using Canvas = massive_moose.services.models.drawing.Canvas;
using InkPresenter = massive_moose.services.models.drawing.InkPresenter;
using TextBlock = massive_moose.services.models.drawing.TextBlock;

namespace massive_moose.services
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
                grid.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(canvasContract.BackgroundColor.A, canvasContract.BackgroundColor.R, canvasContract.BackgroundColor.G, canvasContract.BackgroundColor.B));
                System.Windows.Controls.InkCanvas canvas = new System.Windows.Controls.InkCanvas();
                System.Windows.Controls.InkPresenter inkPresenter = new System.Windows.Controls.InkPresenter();
                canvas.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
                canvas.Children.Add(inkPresenter);

                if (imageSource != null)
                {
                    System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = imageSource;
                    bmp.EndInit();
                    var image = new System.Windows.Controls.Image();
                    image.Source = bmp;
                    image.Stretch = System.Windows.Media.Stretch.Fill;
                    grid.Children.Add(image);
                }

                foreach (var child in canvasContract.Children)
                {
                    Console.WriteLine("Drawing an object: {0}", child);
                    if (child is InkPresenter)
                    {
                        var input = child as InkPresenter;
                        var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                        canvas.EditingMode = InkCanvasEditingMode.Ink;

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
                                        StylusTip = System.Windows.Ink.StylusTip.Ellipse
                                    }));
                        }

                    }else if (child is Line)
                    {
                        var input = child as Line;
                        var brush = input.Brush as SolidColorBrush;
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
                        var brush = input.Brush as SolidColorBrush;
                        var drawingAttributes = new System.Windows.Ink.DrawingAttributes()
                        {
                            Color = System.Windows.Media.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B),
                            Width = input.StrokeThickness,
                            Height = input.StrokeThickness,
                            FitToCurve = true,
                            StylusTip = System.Windows.Ink.StylusTip.Ellipse
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
                        double x, y, width, height;
                        if (input.Width < 0)
                        {
                            x = input.X + input.Width;
                            width = -input.Width;
                        }
                        else
                        {
                            x = input.X;
                            width = input.Width;
                        }
                        if (input.Height < 0)
                        {
                            y = input.Y + input.Height;
                            height = -input.Height;
                        }
                        else
                        {
                            y = input.Y;
                            height = input.Height;
                        }


                        shape.Width = width;
                        shape.Height = height;
                        shape.Stroke=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.StrokeColor.A, input.StrokeColor.R, input.StrokeColor.G, input.StrokeColor.B));
                        shape.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.FillColor.A, input.FillColor.R, input.FillColor.G, input.FillColor.B));
                        shape.StrokeThickness = input.StrokeWidth;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, x);
                        System.Windows.Controls.Canvas.SetTop(shape, y);
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
                        var shape = new System.Windows.Shapes.Ellipse();
                        double x, y, width, height;
                        if (input.Width < 0)
                        {
                            x = input.X + input.Width;
                            width = -input.Width;
                        }
                        else
                        {
                            x = input.X;
                            width = input.Width;
                        }
                        if (input.Height < 0)
                        {
                            y = input.Y + input.Height;
                            height = -input.Height;
                        }
                        else
                        {
                            y = input.Y;
                            height = input.Height;
                        }
                        shape.Width = width;
                        shape.Height = height;
                        shape.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.StrokeColor.A, input.StrokeColor.R, input.StrokeColor.G, input.StrokeColor.B));
                        shape.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(input.FillColor.A, input.FillColor.R, input.FillColor.G, input.FillColor.B));
                        shape.StrokeThickness = input.StrokeWidth;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, x);
                        System.Windows.Controls.Canvas.SetTop(shape, y);
                    } else if (child is TextBlock)
                    {
                        var input = child as TextBlock;
                        var shape = new System.Windows.Controls.TextBlock();
                        shape.Text = input.Text;
                        if (!string.IsNullOrWhiteSpace(input.FontFamily))
                            shape.FontFamily = new System.Windows.Media.FontFamily(input.FontFamily);
                        if (input.FontSizeInPixels != 0)
                        {
                            shape.FontSize = input.FontSizeInPixels * 72 / 96;

                        
                        }
                        if (input.ForcedWidth != 0)
                        {
                            shape.MaxWidth = input.ForcedWidth;
                        }
                        //shape.MaxHeight = input.ForcedHeight;
                        shape.TextWrapping = System.Windows.TextWrapping.Wrap;
                        canvas.Children.Add(shape);
                        System.Windows.Controls.Canvas.SetLeft(shape, input.X);
                        System.Windows.Controls.Canvas.SetTop(shape, input.Y);
                    } else if (child is Eraser)
                    {
                        var input = child as Eraser;
                        var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                        canvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                        canvas.EraserShape = new EllipseStylusShape(input.PointSize, input.PointSize);

                        foreach (var stroke in input.Strokes)
                        {
                            inkPresenter.Strokes.Erase(
                                stroke.StylusPoints.Select(sp => new System.Windows.Point() { X = sp.X, Y = sp.Y}),
                                new EllipseStylusShape(0.01,0.01));
                        }
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
