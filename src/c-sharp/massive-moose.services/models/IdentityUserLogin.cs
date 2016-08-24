namespace massive_moose.services.models
{
    public class IdentityUserLogin
    {
        public virtual int Id { get; protected set; }
        public virtual string LoginProvider { get; set; }

        public virtual string ProviderKey { get; set; }

        public virtual ApplicationUser ApplicationUser { get;set;}
    }
}