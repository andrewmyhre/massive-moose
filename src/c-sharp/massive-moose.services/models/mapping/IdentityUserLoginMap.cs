using FluentNHibernate.Mapping;
namespace massive_moose.services.models.mapping
{
    public class IdentityUserLoginMap : ClassMap<IdentityUserLogin>
    {
        public IdentityUserLoginMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.LoginProvider).Not.Nullable();
            Map(x => x.ProviderKey).Not.Nullable();
            References(x => x.ApplicationUser, "ApplicationUserId").Cascade.None();
        }
    }
}