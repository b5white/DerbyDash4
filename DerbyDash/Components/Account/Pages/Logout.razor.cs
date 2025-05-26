using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account.Pages {
    public partial class Logout {
        protected override async Task OnInitializedAsync() {
            if (await IsLoggedIn()) {
                Logger.LogInformation("OnInitializedAsync called.");
                try {
                    // Sign out the user
                    await SignInManager.SignOutAsync();

                    // Notify the auth state provider that the user has been logged out
                    AuthStateProvider.NotifyUserAuthentication();
                    RedirectManager.RedirectTo("/", forceLoad: true);
                } catch (NavigationException) {
                    throw;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error during logout.");
                }
            } else {
                // If not logged in, just redirect to home
                RedirectManager.RedirectTo("/");
            }
        }

        private async Task<bool> IsLoggedIn() {
	        return await UserService.IsLoggedInAsync();
        }
    }
}