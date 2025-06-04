using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class DeletePersonalData {
        private string? message;
        private ApplicationUser user = default!;
        private bool requirePassword = false;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public required CustomAuthStateProvider AuthStateProvider { get; set; }
        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; set; }
        [Inject]
        public required SignInManager<ApplicationUser> SignInManager { get; set; }
        [Inject]
        internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public required ILogger<ChangePassword> Logger { get; set; }

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            Input ??= new();
            user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            requirePassword = await UserManager.HasPasswordAsync(user);
            return;
        }

        private async Task OnValidSubmitAsync() {
            if (requirePassword && !await UserManager.CheckPasswordAsync(user, Input.Password)) {
                message = "Error: Incorrect password.";
                return;
            }

            IdentityResult result = await UserManager.DeleteAsync(user);
            if (!result.Succeeded) {
                throw new InvalidOperationException("Unexpected error occurred deleting user.");
            }

            await SignInManager.SignOutAsync();
            AuthStateProvider.NotifyUserLogout();

            string userId = await UserManager.GetUserIdAsync(user);
            Logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

            RedirectManager.RedirectToCurrentPage();
        }

        private sealed class InputModel {
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";
        }
    }
}