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

        public WallViewModel()
        {
            _wall = new BrickViewModel[_viewSize, _viewSize];
            for (int i = 0; i < _viewSize; i++)
            {
                for (int j = 0; j < _viewSize; j++)
                {
                    _wall[i,j] = new BrickViewModel()
                    {
                        AddressX= i-(_viewSize/4),
                        AddressY= j- (_viewSize / 4),
                        ViewSpaceX=i,
                        ViewSpaceY=j,
                        Guid =Guid.NewGuid(),Id=int.Parse(string.Concat(Math.Abs(i).ToString(),Math.Abs(j).ToString()))
                    };
                }
            }
        }

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