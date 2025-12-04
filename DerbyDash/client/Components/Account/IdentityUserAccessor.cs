using DerbyDash.Data;
using DerbyDash.Services;

namespace DerbyDash.Components.Account {
    /// <summary>
    /// Client-side user accessor that uses IUserService
    /// </summary>
    internal sealed class IdentityUserAccessor {
        private readonly IUserService _userService;
        private readonly IdentityRedirectManager _redirectManager;

        public IdentityUserAccessor(IUserService userService, IdentityRedirectManager redirectManager) {
            _userService = userService;
            _redirectManager = redirectManager;
        }

        public async Task<ApplicationUser> GetRequiredUserAsync() {
            var user = await _userService.GetCurrentUserAsync();

            if (user is null) {
                _redirectManager.RedirectTo("Account/Login");
                throw new InvalidOperationException("Unable to load user");
            }

            return user;
        }
    }
}
