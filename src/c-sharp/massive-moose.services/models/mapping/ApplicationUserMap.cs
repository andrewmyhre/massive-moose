using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace massive_moose.services.models.mapping
{
    public class ApplicationUserMap : ClassMap<ApplicationUser>
    {
        public ApplicationUserMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.UserName);
            Map(x => x.PasswordHash);
            Map(x => x.Email);
            Map(x => x.EmailConfirmed);
            HasMany<IdentityUserLogin>(x => x.Logins).KeyColumn("ApplicationUserId");
            HasMany<Wall>(x => x.Walls).KeyColumn("ApplicationUserId");
        }
    }
}