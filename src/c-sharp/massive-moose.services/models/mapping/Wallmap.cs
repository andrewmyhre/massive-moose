using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace massive_moose.services.models.mapping
{
    public class WallMap : ClassMap<Wall>
    {
        public WallMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.DateCreated).Not.Nullable();
            Map(x => x.Guid);
            References(x => x.Owner, "OwnerId").Cascade.None();
            Map(x => x.InviteCode);
            HasMany<WallHistoryItem>(x => x.History).KeyColumn("WallId");
        }
    }

    public class WallHistoryItemMap : ClassMap<WallHistoryItem>
    {
        public WallHistoryItemMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.ClientIp);
            Map(x => x.SnapshotImage).Length(9999999);
            Map(x => x.SnapshotImageThumbnail).Length(9999999);
            Map(x => x.SnapshotJson).Length(9999999);
            References(x => x.Wall, "WallId").Cascade.None();
        }
    }
}
