using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace massive_moose.services.models
{
    public class ApplicationUser : IUser<int>
    {
        public ApplicationUser()
        {
            Logins = new List<IdentityUserLogin>();
        }
        public virtual string Email { get; set; }
        public virtual int Id { get; protected set; }
        public virtual string PasswordHash { get; set; }
        public virtual string UserName { get; set; }
        public virtual IList<IdentityUserLogin> Logins { get; set; }
        public virtual bool EmailConfirmed { get; set; }

        public virtual IList<Wall> Walls { get; set; }

        public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}