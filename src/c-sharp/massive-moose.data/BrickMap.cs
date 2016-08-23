using massive_moose.contracts;
using massive_moose.contracts.drawing;

namespace massive_moose.data
{
    public class BrickMap : FluentNHibernate.Mapping.ClassMap<Brick>
    {
        public BrickMap()
        {
            Id(x=>x.Id);
            Map(x=>x.AddressX);
            Map(x=>x.AddressY);
            Map(x=>x.Guid);
            Map(x => x.SnapshotJson).Length(9999999);
        }
    }
}