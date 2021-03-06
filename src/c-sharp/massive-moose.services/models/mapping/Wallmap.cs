﻿using FluentNHibernate.Mapping;
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
            Map(x => x.Label);
            Map(x => x.DateCreated).Not.Nullable();
            Map(x => x.Guid);
            References(x => x.Owner, "OwnerId").Cascade.None();
            Map(x => x.InviteCode);
            Map(x => x.BackgroundImageFilename);
            HasMany<WallHistoryItem>(x => x.History).KeyColumn("WallId");
            HasMany<Brick>(x => x.Bricks).KeyColumn("WallId");
            Map(x => x.ETag);
        }
    }
}
