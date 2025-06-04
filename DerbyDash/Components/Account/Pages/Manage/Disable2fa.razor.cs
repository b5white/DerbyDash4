using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class Disable2fa {
        private ApplicationUser user = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; set; }
        [Inject]
        internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public required ILogger<ChangePassword> Logger { get; set; }

        protected override async Task OnInitializedAsync() {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);

            if (HttpMethods.IsGet(HttpContext.Request.Method) && !await UserManager.GetTwoFactorEnabledAsync(user)) {
                throw new InvalidOperationException("Cannot disable 2FA for user as it's not currently enabled.");
            }
            return;
        }

        private async Task OnSubmitAsync() {
            IdentityResult disable2faResult = await UserManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded) {
                throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
            }

            string userId = await UserManager.GetUserIdAsync(user);
            Logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", userId);
            RedirectManager.RedirectToWithStatus(
                "Account/Manage/TwoFactorAuthentication",
                "2fa has been disabled. You can reenable 2fa when you setup an authenticator app",
                HttpContext);
        }
    }
}