using FluentNHibernate.Mapping;
namespace massive_moose.services.models.mapping
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