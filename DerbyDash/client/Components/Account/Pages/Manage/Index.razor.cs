using DerbyDash.Components.Account;
using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class Index {
        private ApplicationUser? user;
        private string? username = null;
        private string? phoneNumber = null;
        private string? message = null;

        [Inject]
        public required IUserService UserService { get; set; }

        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;

        [Inject]
        public required AuthenticationStateProvider AuthStateProvider { get; set; }

        private CustomAuthStateProvider CustomAuthStateProvider => (CustomAuthStateProvider)AuthStateProvider;

        [Inject]
        public required ILogger<Index> Logger { get; set; }

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            try {
                user = await UserService.GetCurrentUserAsync();
                if (user == null) {
                    RedirectManager.RedirectTo("Account/Login");
                    return;
                }

                username = user.UserName ?? user.Email;
                // Phone number not available from API yet
                phoneNumber = null;

                Input.PhoneNumber ??= phoneNumber;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading user profile");
                message = "Error loading profile. Please try again.";
            }
        }

        private async Task OnValidSubmitAsync() {
            try {
                // Phone number update requires API endpoint
                // For now, just show a message
                message = "Phone number update is not yet available. This feature will be added soon.";
                Logger.LogInformation("Phone number update requested but not yet implemented");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error updating profile");
                message = "Error updating profile. Please try again.";
            }
        }

        private sealed class InputModel {
            [Phone]
            [Display(Name = "Phone number")]
            public string? PhoneNumber { get; set; }
        }
    }
}
