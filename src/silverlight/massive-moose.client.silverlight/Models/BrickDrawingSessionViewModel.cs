using System;
using System.ComponentModel;

namespace massive_moose.client.silverlight.Models
{
    public class BrickDrawingSessionViewModel : INotifyPropertyChanged
    {
        private BrickViewModel _brick;
        private Guid _sessionToken;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public BrickViewModel Brick
        {
            get { return _brick; }
            set { _brick = value; OnPropertyChanged("Brick"); }
        }

        public Guid SessionToken
        {
            get { return _sessionToken; }
            set { _sessionToken = value; OnPropertyChanged("SessionToken"); }
        }

        public BrickDrawingSessionViewModel()
        {
            
        }

        public BrickDrawingSessionViewModel(BrickViewModel brick, Guid sessionToken)
        {
            _brick = brick;
            _sessionToken = sessionToken;
        }
    }
}