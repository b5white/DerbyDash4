using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class Index {
        private ApplicationUser user = default!;
        private string? username = null;
        private string? phoneNumber = null;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; set; }
        [Inject]
        public required SignInManager<ApplicationUser> SignInManager { get; set; }
        [Inject]
        internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject]
        public required CustomAuthStateProvider AuthStateProvider { get; set; }
        [Inject]
        public required ILogger<ChangePassword> Logger { get; set; }

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            username = await UserManager.GetUserNameAsync(user);
            phoneNumber = await UserManager.GetPhoneNumberAsync(user);

            Input.PhoneNumber ??= phoneNumber;
            return;
        }

        private async Task OnValidSubmitAsync() {
            if (Input.PhoneNumber != phoneNumber) {
                IdentityResult setPhoneResult = await UserManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded) {
                    RedirectManager.RedirectToCurrentPageWithStatus("Error: Failed to set phone number.", HttpContext);
                }
            }

            await SignInManager.RefreshSignInAsync(user);
            AuthStateProvider.NotifyUserLogin();
            RedirectManager.RedirectToCurrentPageWithStatus("Your profile has been updated", HttpContext);
        }

        private sealed class InputModel {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }
        }
    }
}