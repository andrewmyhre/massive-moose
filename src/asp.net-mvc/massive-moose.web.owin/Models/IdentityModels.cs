using massive_moose.services.models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace massive_moose.web.owin.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class UserManager : UserManager<ApplicationUser, int>
    {
        public UserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
            UserValidator = new UserValidator<ApplicationUser, int>(this);
            PasswordValidator = new PasswordValidator() { RequiredLength = 6 };
        }
    }
    public class SignInManager : SignInManager<ApplicationUser, int>
    {
        public SignInManager(UserManager<ApplicationUser, int> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager) { }

        public void SignOut()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }
    }
}