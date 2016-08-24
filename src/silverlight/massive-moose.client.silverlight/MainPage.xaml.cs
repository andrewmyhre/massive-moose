using massive_moose.client.silverlight.Models;
using Newtonsoft.Json;
using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace massive_moose.client.silverlight
{
    public partial class MainPage : UserControl
    {
        private WallViewModel _wallVm= null;
        private IKernel _kernel = null;
        private IMassiveMooseService _massiveMooseService = null;

        public MainPage()
        {
            InitializeComponent();
            _massiveMooseService = new MassiveMooseService();
            _kernel = new StandardKernel(new MassiveMooseNinjectModule());
            _wallVm = _kernel.Get<WallViewModel>();
            

            BrickCanvas.OnBrickUpdated += (sender, args) =>
            {
                var canvas = sender as BrickCanvas;
                var drawingSession = canvas.DataContext as Models.BrickDrawingSessionViewModel;
                drawingSession.Brick.Info = "Updated!";
                drawingSession.Brick.ImageUrl = string.Format("{0}/v1/image/{1}/{2}", App.MMApiBaseUrl,
                    drawingSession.Brick.AddressX, drawingSession.Brick.AddressY);
                UpdateActiveSessions();

                ((BrickCanvas) sender).Visibility = Visibility.Collapsed;
                drawingSession.Brick.InvalidateImage();
                drawingSession.Brick.Locked = false;
                canvas.ClearCanvas();
            };

            BrickCanvas.OnCancel += (sender, args) =>
            {
                var canvas = sender as BrickCanvas;
                var drawingSession = canvas.DataContext as Models.BrickDrawingSessionViewModel;
                ((BrickCanvas)sender).Visibility = Visibility.Collapsed;
                canvas.ClearCanvas();
                drawingSession.Brick.ImageUrl = string.Format("{0}/v1/image/{1}/{2}", App.MMApiBaseUrl,
                    drawingSession.Brick.AddressX, drawingSession.Brick.AddressY);
                drawingSession.Brick.Locked = false;
            };

            

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateAllData();
        }

        private void UpdateAllData()
        {
            _massiveMooseService.OnGotBricks += (o, args) =>
            {
                _wallVm.Update(args.Bricks);
                for (int x = 0; x < args.Bricks.GetLength(0); x++)
                {
                    for (int y = 0; y < args.Bricks.GetLength(1); y++)
                    {
                        var brick = args.Bricks[x, y];
                        if (brick != null)
                        {
                            var bvm = _wallVm.Bricks.SingleOrDefault(b => b.AddressX == brick.AddressX && b.AddressY == brick.AddressY);
                            //bvm.ImageUrl = App.MMApiBaseUrl + "/v1/image/"+brick.AddressX+"/"+brick.AddressY;
                            if (brick.Guid != Guid.Empty)
                            {
                                bvm.Info=bvm.ImageUrl = "http://placehold.it/200x100?text=" + x + ',' + y;
                            }

                            var bUc = new Brick();
                            bUc.RenderTransform = new TranslateTransform() { X = y % 2 == 0 ? 0 : 100, Y = 0 };
                            bUc.DataContext = bvm;
                            Wall.Children.Add(bUc);

                            Grid.SetColumn(bUc, x);
                            Grid.SetRow(bUc, y);
                            bUc.Select += (s, args2) =>
                            {
                                try
                                {
                                    OpenDrawingSession(bvm);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                }

                            };
                        }
                    }
                }
            };
            _massiveMooseService.GetBricksAround(0, 0);

            UpdateActiveSessions();
        }

        private void UpdateActiveSessions()
        {
            foreach(var b in _wallVm.Bricks) {  b.Locked=false;}
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) =>
            {
                var sessions = JsonConvert.DeserializeObject<List<DrawingSession>>(args.Result);
                foreach (var session in sessions)
                {
                    var brick = _wallVm.Bricks.SingleOrDefault(b => b.AddressX == session.AddressX && b.AddressY == session.AddressY);
                    if (brick != null)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            brick.Locked = true;
                        });
                    }
                }
            };
            wc.DownloadStringAsync(new Uri(App.MMApiBaseUrl+"/v1/image/sessions"));
        }

        private void OpenDrawingSession(BrickViewModel b)
        { 
            HttpWebRequest request = HttpWebRequest.CreateHttp(string.Format("{0}/v1/image/begin/{1}/{2}", App.MMApiBaseUrl, b.AddressX, b.AddressY));
            request.Method = "POST";
            request.ContentType = "application/json";
            request.BeginGetRequestStream((ar) =>
            {
                var requestStream = request.EndGetRequestStream(ar);
                requestStream.Close();
                request.BeginGetResponse((response_result) =>
                {
                    try
                    {
                        using (var response = request.EndGetResponse(response_result) as System.Net.HttpWebResponse)
                        {
                            if (response.StatusCode == HttpStatusCode.Conflict)
                            {
                                b.Locked = true;
                                return;
                            }
                            using (var responseStream = response.GetResponseStream())
                            using (var reader = new System.IO.StreamReader(responseStream))
                            {
                                string responseText = reader.ReadToEnd().Trim('"');
                                Guid sessionToken = Guid.Empty;
                                if (Guid.TryParse(responseText, out sessionToken))
                                {
                                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        b.Locked = true;
                                        StartDrawing(
                                            new BrickDrawingSessionViewModel(
                                                (BrickViewModel) response_result.AsyncState, sessionToken));
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }, ar.AsyncState);
            }, b);
        }

        private void StartDrawing(BrickDrawingSessionViewModel drawingSession)
        {
            BrickCanvas.ClearCanvas();
            BrickCanvas.Visibility = Visibility.Visible;
            BrickCanvas.DataContext = drawingSession;
        }
    }
}
