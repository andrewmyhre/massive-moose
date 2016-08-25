using FluentNHibernate.Mapping;

namespace massive_moose.services.models.mapping
{
    public class BrickMap : ClassMap<Brick>
    {
        public BrickMap()
        {
            Id(x=>x.Id);
            Map(x=>x.AddressX);
            Map(x=>x.AddressY);
            Map(x=>x.Guid);
            Map(x => x.SnapshotJson).Length(9999999);
            Map(x => x.LastUpdated).Nullable();
            References(x => x.Wall, "WallId").Cascade.None();
            References(x => x.DrawingSession, "DrawingSessionId").Cascade.None();
        }
    }
}