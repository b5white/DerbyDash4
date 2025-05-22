using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class TwoFactorAuthentication {
        private bool canTrack = false;
        private bool hasAuthenticator = false;
        private int recoveryCodesLeft = 0;
        private bool is2faEnabled = false;
        private bool isMachineRemembered = false;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject]
        public SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public ILogger<ChangePassword> Logger { get; set; } = default!;

        protected override async Task OnInitializedAsync() {
            //var user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            //canTrack = HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;
            //hasAuthenticator = await UserManager.GetAuthenticatorKeyAsync(user) is not null;
            //is2faEnabled = await UserManager.GetTwoFactorEnabledAsync(user);

            await Task.CompletedTask; // Just to use 'await'
            return;
            //isMachineRemembered = await SignInManager.IsTwoFactorClientRememberedAsync(user);
            //recoveryCodesLeft = await UserManager.CountRecoveryCodesAsync(user);
        }

        private async Task OnSubmitForgetBrowserAsync() {
            await SignInManager.ForgetTwoFactorClientAsync();

            RedirectManager.RedirectToCurrentPageWithStatus(
                "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.",
                HttpContext);
        }
    }
}