using System;

namespace massive_moose.services.models
{
    public class WallHistoryItem
    {
        public virtual int Id { get; protected set; }
        public virtual string SnapshotJson { get; set; }
        public virtual string ClientIp { get; set; }
        public virtual byte[] SnapshotImage { get; set; }
        public virtual byte[] SnapshotImageThumbnail { get; set; }
        public virtual Wall Wall { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual DrawingSession DrawingSession { get; set; }

    }
}