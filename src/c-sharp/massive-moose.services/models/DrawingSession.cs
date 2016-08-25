using System;

namespace massive_moose.services.models
{
    public class DrawingSession
    {
        public DrawingSession()
        {
            
        }
        public DrawingSession(Wall wall, int addressX, int addressY)
        {
            this.Wall = wall;
            this.AddressX = addressX;
            this.AddressY = addressY;
            this.SessionToken = Guid.NewGuid();
            Opened = DateTime.Now;
        }
        public virtual DateTime Opened { get; set; }
        public virtual Guid SessionToken { get; set; }
        public virtual int AddressX { get; set; }
        public virtual int AddressY { get; set; }
        public virtual bool Closed { get; set; }
        public virtual int Id { get; set; }
        public virtual Wall Wall { get; set; }
    }
}