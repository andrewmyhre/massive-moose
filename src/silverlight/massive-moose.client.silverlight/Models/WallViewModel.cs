using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace massive_moose.client.silverlight.Models
{
    public class WallViewModel : INotifyPropertyChanged
    {
        private int _viewSize = 12;
        private ObservableCollection<BrickViewModel> _bricks = new ObservableCollection<BrickViewModel>();
        private BrickViewModel[,] _wall = null;

        public ObservableCollection<BrickViewModel> Bricks
        {
            get { return _bricks; }
        }

        public BrickViewModel[,] Wall {  get { return _wall; } }

        public WallViewModel()
        {

        }

        public void Update(massive_moose.contracts.Brick[,] bricks)
        {
            _wall = new BrickViewModel[bricks.GetLength(0), bricks.GetLength(1)];
            _bricks.Clear();
            for (int x = 0; x < bricks.GetLength(0); x++)
            {
                for (int y = 0; y < bricks.GetLength(1); y++)
                {
                    _wall[x,y] = new BrickViewModel(bricks[x,y]);
                    _bricks.Add(_wall[x, y]);
                }
            }
            RaisePropertyChanged("Bricks");
            RaisePropertyChanged("Wall");
        }

        [Obsolete("Use Update()", true)]
        public void Load(int minX, int minY, int maxX, int maxY)
        {
            _bricks.Clear();
            for (int i = 0; i < _viewSize; i++)
            {
                for (int j = 0; j < _viewSize; j++)
                {
                    _bricks.Add(_wall[i,j]);
                }
            }
            RaisePropertyChanged("Bricks");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}