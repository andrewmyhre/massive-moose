using FluentNHibernate.Mapping;
using massive_moose.contracts;
using massive_moose.contracts.drawing;

namespace massive_moose.data
{
    public class DrawingSessionMap : ClassMap<DrawingSession>
    {
        public DrawingSessionMap()
        {
            Id(x => x.Id);
            Map(x => x.AddressX);
            Map(x => x.AddressY);
            Map(x => x.Closed);
            Map(x => x.Opened);
            Map(x => x.SessionToken);
        }
    }
}