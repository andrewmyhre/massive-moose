using FluentNHibernate.Mapping;

namespace massive_moose.services.models.mapping
{
    public class WallHistoryItemMap : ClassMap<WallHistoryItem>
    {
        public WallHistoryItemMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.ClientIp);
            Map(x => x.SnapshotImage).Length(9999999);
            Map(x => x.SnapshotImageThumbnail).Length(9999999);
            Map(x => x.SnapshotJson).Length(9999999);
            Map(x => x.Timestamp).Not.Nullable();
            References(x => x.DrawingSession, "DrawingSessionId").Cascade.None();
            References(x => x.Wall, "WallId").Cascade.None();
        }
    }
}