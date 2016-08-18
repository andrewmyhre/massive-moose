using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class Toolbox : UserControl
    {
        public double BrushSize
        {
            get
            {
                double brushSize = 0;
                if (double.TryParse(BrushSizeTextBox.Text, out brushSize))
                {
                    return brushSize;
                }
                return 0;
            }
            set
            {
                if (BrushSizeTextBox == null) return;
                BrushSizeTextBox.Text = value.ToString();
            }
        }

        public Color BrushColor
        {
            get { return ColorPicker.Color; }
        }

        public Brush Brush
        {
            get
            {
                if (BrushSizeIndicator != null)
                    return BrushSizeIndicator.Fill;
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            set {
                if (BrushSizeIndicator==null)
                    return;
                BrushSizeIndicator.Fill = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Toolbox()
        {
            InitializeComponent();
            /*BrushSizeTextBox.TextChanged += (sender, args) =>
            {
                double brushSize = 0;
                if (double.TryParse(BrushSizeTextBox.Text, out brushSize))
                {
                    BrushSizeSlider.Value = brushSize;
                }
            };*/

            //this.BrushSizeSlider.ValueChanged += (sender, args) => { OnPropertyChanged("BrushSize"); };
            ColorPicker.ColorChanged += (sender, args) => { BrushSizeIndicator.Fill = new SolidColorBrush(ColorPicker.Color);};
        }

        public void SetColor(Color color)
        {
            ColorPicker.Color = color;
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
