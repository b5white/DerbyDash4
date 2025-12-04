using DerbyDash.Components.Account;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DerbyDash.Components.Account.Pages {
    public partial class Logout {
        [Inject]
        public required ApiAuthService AuthService { get; set; }

        [Inject]
        public required AuthenticationStateProvider AuthStateProvider { get; set; }

        private CustomAuthStateProvider CustomAuthStateProvider => (CustomAuthStateProvider)AuthStateProvider;

        [Inject]
        public required IUserService UserService { get; set; }

        [Inject]
        public required ILogger<Logout> Logger { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }

        protected override async Task OnInitializedAsync() {
            try {
                if (await UserService.IsLoggedInAsync()) {
                    Logger.LogInformation("OnInitializedAsync called.");
                    
                    // Call API to logout
                    await AuthService.LogoutAsync();

                    // Notify the auth state provider that the user has been logged out
                    CustomAuthStateProvider.NotifyUserLogout();
                    
                    Logger.LogInformation("User logged out successfully.");
                }
            } catch (NavigationException) {
                throw;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error during logout.");
            }
            
            // Redirect to Welcome page
            NavManager.NavigateTo("/Welcome", forceLoad: true);
        }
    }
}
