using System;
using System.Collections.Generic;
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
    public partial class Brick : UserControl
    {
        public event EventHandler<SelectBrickEventArgs> Select;
        public Brick()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Select != null)
                Select(this, new SelectBrickEventArgs(this.DataContext as massive_moose.contracts.Brick));
        }
    }

    public class SelectBrickEventArgs : EventArgs
    {
        public SelectBrickEventArgs(massive_moose.contracts.Brick brick)
        {
            this.Brick = brick;
        }
        public massive_moose.contracts.Brick Brick { get; private set; }
    }
}
