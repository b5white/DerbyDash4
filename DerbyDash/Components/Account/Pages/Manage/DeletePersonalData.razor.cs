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
        public CustomAuthStateProvider AuthStateProvider { get; set; } = default!;
        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject]
        public SignInManager<ApplicationUser> SignInManager { get; set; } = default!;
        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;
        [Inject] public ILogger<ChangePassword> Logger { get; set; } = default!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            Input ??= new();
            //user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            //requirePassword = await UserManager.HasPasswordAsync(user);
            await Task.CompletedTask; // Just to use 'await'
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
            AuthStateProvider.NotifyUserAuthentication();

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