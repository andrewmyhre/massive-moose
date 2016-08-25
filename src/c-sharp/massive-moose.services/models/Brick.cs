using System;

namespace massive_moose.services.models
{
    public class Brick
    {
        public Brick()
        {
            Guid = Guid.NewGuid();
        }
        public virtual int AddressX { get; set; }
        public virtual int AddressY { get; set; }
        public virtual Guid Guid { get; set; }
        public virtual int Id { get; set; }
        public virtual string SnapshotJson { get; set; }
        public virtual Wall Wall { get; set; }
        public virtual DrawingSession DrawingSession { get; set; }
        public virtual DateTime? LastUpdated { get; set; }
    }
}