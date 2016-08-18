using massive_moose.client.silverlight.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace massive_moose.client.silverlight
{
    public class GotBricksEventArgs : EventArgs
    {
        public IEnumerable<massive_moose.contracts.Brick> Bricks { get; set; }

        public GotBricksEventArgs(IEnumerable<massive_moose.contracts.Brick> bricks)
        {
            Bricks = bricks;
        }
    }

    public class MassiveMooseService : IMassiveMooseService
    {
        public event EventHandler<GotBricksEventArgs> OnGotBricks;
        public void GetBricksInRange(int minX, int minY, int maxX, int maxY)
        {
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (sender, args) =>
            {
                var bricks = JsonConvert.DeserializeObject<List<massive_moose.contracts.Brick>>(args.Result);
                if (OnGotBricks != null)
                    OnGotBricks(this, new GotBricksEventArgs(bricks));
            };
            webClient.DownloadStringAsync(new Uri(App.MMApiBaseUrl+"/v1/wall"));
        }
    }
}
