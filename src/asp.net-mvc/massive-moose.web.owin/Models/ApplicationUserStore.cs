using massive_moose.services.models;
using Microsoft.AspNet.Identity;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace massive_moose.web.owin.Models
{
    public class ApplicationUserStore : IUserStore<ApplicationUser, int>, IUserPasswordStore<ApplicationUser, int>,
        IUserLockoutStore<ApplicationUser, int>, IUserTwoFactorStore<ApplicationUser, int>, IUserLoginStore<ApplicationUser, int>,
        IUserEmailStore<ApplicationUser, int>
    {
        private readonly ISession _session = null;

        public ApplicationUserStore(ISession session)
        {
            _session = session;
        }

        public Task AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            var u = _session.Get<ApplicationUser>(user.Id);
            var identityUserLogin = new IdentityUserLogin() {ProviderKey=login.ProviderKey, LoginProvider = login.LoginProvider};
            identityUserLogin.ApplicationUser = u;
            u.Logins.Add(identityUserLogin);
            _session.SaveOrUpdate(identityUserLogin);
            _session.SaveOrUpdate(u);
            _session.Flush();
            return Task.CompletedTask;
        }

        public Task CreateAsync(ApplicationUser user)
        {
            return Task.Run(() => { _session.SaveOrUpdate(user);_session.Flush(); });
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            return Task.Run(() => { _session.Delete(user);_session.Flush(); });
        }

        public void Dispose()
        {
            
        }

        public Task<ApplicationUser> FindAsync(UserLoginInfo login)
        {
            var user = _session.CreateCriteria<ApplicationUser>("u")
                .CreateCriteria("u.Logins", JoinType.InnerJoin)
                .Add(Restrictions.Eq("LoginProvider", login.LoginProvider))
                .Add(Restrictions.Eq("ProviderKey", login.ProviderKey))
                .UniqueResult<ApplicationUser>();
            return Task.FromResult(user);
        }

        public Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return Task<ApplicationUser>.Run(() => _session.CreateCriteria<ApplicationUser>()
                .Add(Restrictions.Eq("Email", email))
                .UniqueResult<ApplicationUser>());
        }

        public Task<ApplicationUser> FindByIdAsync(int userId)
        {
            return Task<ApplicationUser>.Run(() => _session.Get<ApplicationUser>(userId));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task<ApplicationUser>.Run(() => _session.CreateCriteria<ApplicationUser>()
                .Add(Restrictions.Eq("UserName", userName))
                .UniqueResult<ApplicationUser>());
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(ApplicationUser user)
        {
            return Task.FromResult(DateTimeOffset.MaxValue);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            var u = _session.Get<ApplicationUser>(user.Id);
            IList<UserLoginInfo> logins = new List<UserLoginInfo>();
            var d = u.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
            foreach (var l in d)
            {
                logins.Add(l);
            }
            return Task.FromResult(logins);
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return Task.FromResult(true);
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            var u = _session.Get<ApplicationUser>(user.Id);
            var l =
                u.Logins.SingleOrDefault(
                    x => x.ProviderKey == login.ProviderKey && x.LoginProvider == login.LoginProvider);
            u.Logins.Remove(l);
            _session.SaveOrUpdate(u);
            _session.Flush();
            return Task.CompletedTask;
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.CompletedTask;
        }

        public Task SetEmailAsync(ApplicationUser user, string email)
        {
            return Task.Run(() =>
            {
                var u = _session.Get<ApplicationUser>(user.Id);
                u.Email = email;
                _session.SaveOrUpdate(u);
                _session.Flush();
            });
        }

        public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed)
        {
            return Task.Run(() =>
            {
                var u = _session.Get<ApplicationUser>(user.Id);
                u.EmailConfirmed = confirmed;
                _session.SaveOrUpdate(u);
                _session.Flush();
            });
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled)
        {
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset lockoutEnd)
        {
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            return Task.Run(() => user.PasswordHash = passwordHash);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            return Task.Run(() => { _session.SaveOrUpdate(user);_session.Flush(); });
        }
    }
}