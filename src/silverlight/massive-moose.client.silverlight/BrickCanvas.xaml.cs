using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using massive_moose.contracts;
using Newtonsoft.Json;
using Brush = System.Windows.Media.Brush;
using Canvas = System.Windows.Controls.Canvas;
using Point = System.Windows.Point;
using Polyline = System.Windows.Shapes.Polyline;
using System.Runtime.Serialization;
using System.Xml;

namespace massive_moose.client.silverlight
{
    public partial class BrickCanvas : UserControl
    {
        private bool _drawing;
        private Path _path = new Path();
        private Path _brush=new Path() {Data=new EllipseGeometry(), StrokeThickness=0};
        private System.Windows.Ink.Stroke _currentLine = null;
        private GeometryGroup _geometry = null;
        private StringBuilder _log = new StringBuilder();
        private WriteableBitmap _writeableBitmap = null;

        public event EventHandler OnBrickUpdated;
        public event EventHandler OnCancel;
        public BrickCanvas()
        {
            InitializeComponent();
            this.Loaded += BrickCanvas_Loaded;
            BrushLayer.MouseLeftButtonDown += BrickCanvas_MouseLeftButtonDown;
            BrushLayer.MouseLeftButtonUp += BrickCanvas_MouseLeftButtonUp;
            BrushLayer.MouseMove += BrickCanvas_MouseMove;

            DrawingSpace.MouseLeftButtonDown += BrickCanvas_MouseLeftButtonDown;
            DrawingSpace.MouseLeftButtonUp += BrickCanvas_MouseLeftButtonUp;
            DrawingSpace.MouseMove += BrickCanvas_MouseMove;

            DrawingSpace.LayoutUpdated += (sender, args) =>
            {
                if (BaseLayer == null) return;
                if (_writeableBitmap == null)
                {
                    _writeableBitmap = new WriteableBitmap(DrawingSpace, null);
                    BaseLayer.Background = new ImageBrush() {ImageSource = _writeableBitmap};
                }
                else
                {
                    _writeableBitmap.Invalidate();
                }

            };

            
            App.Current.Host.Content.Resized += (sender, args) =>
            {
                this.Width = App.Current.Host.Content.ActualWidth;
                this.Height = App.Current.Host.Content.ActualHeight;
                DrawingSpace.Width = UsableArea.Width;
                DrawingSpace.Height = UsableArea.Height;
                BrushLayer.Width = UsableArea.Width;
                BrushLayer.Height = UsableArea.Height;
            };

            Finish.Click += Finish_Click;
            Cancel.Click += Cancel_Click;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            UploadResultContainer.Visibility = Visibility.Collapsed;
            ClearCanvas();
            if (OnCancel != null)
                OnCancel(this, e);
        }

        WebClient client = new WebClient();
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            var drawingSession = this.DataContext as Models.BrickDrawingSessionViewModel;
            var canvas = new GeometryToContractMapper().Map(DrawingSpace);

            var sb = new StringBuilder();
            System.Xml.XmlWriterSettings settings = new XmlWriterSettings()
            {
                
            };
            using (var xmlWriter = System.Xml.XmlWriter.Create(sb, settings))
            {
                new DataContractSerializer(typeof(massive_moose.contracts.Canvas)).WriteObject(xmlWriter, canvas);
                xmlWriter.Flush();
            }
            var data = sb.ToString();

            HttpWebRequest request = HttpWebRequest.CreateHttp(App.MMApiBaseUrl+"/v1/image/receive/" + drawingSession.SessionToken.ToString());
            request.ContentType = "application/json";
            request.Method = "POST";
            request.BeginGetRequestStream((ar) =>
            {
                var requestStream = request.EndGetRequestStream(ar);
                using (var writer = new System.IO.StreamWriter(requestStream))
                {
                    writer.Write((string)ar.AsyncState);
                    writer.Flush();
                    writer.Close();
                }

                request.BeginGetResponse((response_result) =>
                {
                    try
                    {
                        using (var response = request.EndGetResponse(response_result))
                        {
                            using (var responseStream = response.GetResponseStream())
                            using (var reader = new System.IO.StreamReader(responseStream))
                            {
                                string responseText = reader.ReadToEnd();
                            }
                        }

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            UploadResultContainer.Visibility = Visibility.Collapsed;
                            if (OnBrickUpdated != null)
                                OnBrickUpdated(this, e);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                },null);
            }, data);

            UploadResultContainer.Visibility = Visibility.Visible;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                UploadResult.Dispatcher.BeginInvoke(
                                       () => UploadResult.Text = "Uploading...");
            });
            

        }

        internal void ClearCanvas()
        {
            DrawingSpace.Strokes .Clear();
            toolbox.BrushSize = 4;
            toolbox.Brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
            toolbox.SetColor(System.Windows.Media.Color.FromArgb(255,0,0,0));
            UpdateBrush(toolbox.BrushSize, toolbox.Brush, new Point(0, 0));
        }

        private void BrickCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            _geometry = new GeometryGroup();
            _path = new Path();
            _path.StrokeThickness = toolbox.BrushSize;
            _path.Stroke = toolbox.Brush;
            _path.StrokeLineJoin = PenLineJoin.Round;

            _path.Data = _geometry;

            BrushLayer.Children.Add(_brush);
            ClearCanvas();
        }

        void SetBindings()
        {
            Binding brushSizeBinding = new Binding("BrushSize");
            brushSizeBinding.Source = toolbox;
            toolbox.SetBinding(EllipseGeometry.RadiusXProperty, brushSizeBinding);
            toolbox.SetBinding(EllipseGeometry.RadiusYProperty, brushSizeBinding);
        }

        private void UpdateBrush(double size, Brush brush, Point position)
        {
            ((EllipseGeometry)_brush.Data).Center = position;
            ((EllipseGeometry) _brush.Data).RadiusX = ((EllipseGeometry) _brush.Data).RadiusY = size/2;
            _brush.Fill = brush;
        }

        private Point _lastMousePosition = new Point(-1,-1);

        private void BrickCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingSpace);
            UpdateBrush(toolbox.BrushSize, toolbox.Brush, mousePos);

            if (_drawing)
            {
                _log.AppendFormat("({3}) Continue draw from {0} to {1}{2}", _lastMousePosition, mousePos,
                    Environment.NewLine, ((Canvas)sender).Name);

                _currentLine.StylusPoints.Add(new System.Windows.Input.StylusPoint(mousePos.X, mousePos.Y) { PressureFactor = 1.0f });
                coords.Text = mousePos.ToString();
                _lastMousePosition = mousePos;
            }
            else
            {
                //log.Text += "Not drawing" + Environment.NewLine;
            }
        }

        private void BrickCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingSpace);

            _log.AppendFormat("({2}) Completed draw at {0}{1}", mousePos, Environment.NewLine, ((Canvas)sender).Name);
            e.Handled = true;
            _drawing = false;
            DrawingSpace.ReleaseMouseCapture();
            _path = null;
            _currentLine = null;

            /*DrawingSpace.Children.Add(new Path()
            {
                Fill = toolbox.Brush,
                StrokeThickness = 0,
                Data = new EllipseGeometry() { Center = mousePos, RadiusX = toolbox.BrushSize/2, RadiusY = toolbox.BrushSize/2 }
            });*/
            BrushLayer.Opacity = 1;
        }

        private void BrickCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingSpace);
            BrushLayer.Opacity = 0;
            _log.AppendFormat("({2}) Starting draw at {0}{1}", mousePos, Environment.NewLine, 
                ((Canvas)sender).Name);
            e.Handled = true;
            _drawing = true;
            DrawingSpace.CaptureMouse();
            _lastMousePosition = mousePos;

            /*DrawingSpace.Children.Add(new Path()
            {
                Fill=toolbox.Brush,
                StrokeThickness = 0,
                Data = new EllipseGeometry() { Center=mousePos, RadiusX = toolbox.BrushSize/2, RadiusY=toolbox.BrushSize/2}
            });*/

            _currentLine = new System.Windows.Ink.Stroke();
            _currentLine.DrawingAttributes.Color = toolbox.BrushColor;
            _currentLine.DrawingAttributes.Width = _currentLine.DrawingAttributes.Height = toolbox.BrushSize;
            _currentLine.DrawingAttributes.OutlineColor = toolbox.BrushColor;
            _currentLine.StylusPoints.Add(new System.Windows.Input.StylusPoint(mousePos.X, mousePos.Y) {PressureFactor=1.0f});
            DrawingSpace.Strokes.Add(_currentLine);
        }
    }
}
