using System;
using System.ComponentModel;
using System.Windows;

namespace massive_moose.client.silverlight.Models
{
    public class BrickViewModel : INotifyPropertyChanged
    {
        private int _addressX;
        private int _addressY;
        private Guid _guid;
        private int _id;
        private string _info;
        private int _viewSpaceX;
        private int _viewSpaceY;
        private bool _locked;
        private Visibility _lockedVisibility;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int AddressX
        {
            get { return _addressX; }
            set { _addressX = value; OnPropertyChanged("AddressX"); }
        }

        public int AddressY
        {
            get { return _addressY; }
            set { _addressY = value; OnPropertyChanged("AddressY"); }
        }

        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; OnPropertyChanged("Guid"); }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged("Id"); }
        }

        public string AddressLabel { get { return string.Concat(AddressX.ToString(), ":", AddressY.ToString()); } }
        private static Random random = new Random();
        private int _randomNumber = random.Next(9999999);
        private bool _enabled=true;
        private string _imageUrl;

        public string ImageUrl
        {
            get { return _imageUrl; }
            set { _imageUrl = value; OnPropertyChanged("ImageUrl");}
        }

        public string Info
        {
            get { return _info; }
            set { _info = value; OnPropertyChanged("Info"); }
        }

        public int ViewSpaceX
        {
            get { return _viewSpaceX; }
            internal set { _viewSpaceX = value; }
        }

        public int ViewSpaceY
        {
            get { return _viewSpaceY; }
            internal set { _viewSpaceY = value; }
        }

        public Visibility LockedVisibility
        {
            get { return _lockedVisibility; }
            set { _lockedVisibility = value; OnPropertyChanged("LockedVisibility"); }
        }

        public bool Locked
        {
            get { return _locked; }
            internal set
            {
                _locked = value;
                OnPropertyChanged("Locked");
                LockedVisibility = _locked ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; OnPropertyChanged("Enabled"); }
        }

        public BrickViewModel()
        {
            _lockedVisibility = Visibility.Collapsed;
        }

        internal void InvalidateImage()
        {
            _randomNumber = random.Next(9999999);
            OnPropertyChanged("ImageUrl");
        }

        public BrickViewModel(Brick brick)
        {
            this.AddressX = brick.AddressX;
            this.AddressY = brick.AddressY;
            this.Guid = brick.Guid;
            this.Info = brick.Info;
            _lockedVisibility = Visibility.Collapsed;
        }
    }
}