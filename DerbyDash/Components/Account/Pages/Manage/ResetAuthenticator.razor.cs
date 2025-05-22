using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class ResetAuthenticator {
        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;
        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject]
        public SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        //       [Inject]
        //       internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        //       [Inject]
        //       internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public ILogger<ChangePassword> Logger { get; set; } = default!;

        private async Task OnSubmitAsync() {
            //var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            //await UserManager.SetTwoFactorEnabledAsync(user, false);
            //await UserManager.ResetAuthenticatorKeyAsync(user);
            //var userId = await UserManager.GetUserIdAsync(user);
            //Logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", userId);

            //await SignInManager.RefreshSignInAsync(user);

            //RedirectManager.RedirectToWithStatus(
            //    "Account/Manage/EnableAuthenticator",
            //    "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.",
            //    HttpContext);
            await Task.CompletedTask; // Just to use 'await'
            return;
        }
    }
}