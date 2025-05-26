using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account.Pages {
    public partial class Logout {
        protected override async Task OnInitializedAsync() {
            if (await UserService.IsLoggedInAsync()) {
                Logger.LogInformation("OnInitializedAsync called.");
                try {
                    // Sign out the user
                    await SignInManager.SignOutAsync();

                    // Notify the auth state provider that the user has been logged out
                    AuthStateProvider.NotifyUserAuthentication();
                    Logger.LogInformation("User logged out successfully.");
                } catch (NavigationException) {
                    throw;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error during logout.");
                }
            }
            // If not logged in, just redirect to home
            RedirectManager.RedirectTo("/Welcome", forceLoad: true);
        }
    }
}