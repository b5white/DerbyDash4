using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace DerbyDash.Components.Layout {
    public partial class NavMenu: IDisposable {
        private string? currentUrl;

        protected override void OnInitialized() {
            currentUrl = NavManager.ToBaseRelativePath(NavManager.Uri);
            NavManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
            currentUrl = NavManager.ToBaseRelativePath(e.Location);
            StateHasChanged();
        }

        // Method to close the mobile menu
        private async Task CloseMenu() {
            try {
                await JSRuntime.InvokeVoidAsync("removeClass", navScrollableElement, "show-menu");
            } catch (Exception) {
                // Ignore JS interop errors - menu might already be closed
            }
        }

        // Handle logout directly from the NavMenu
        private async Task HandleLogout() {
            try {
                // Close the mobile menu first
                await CloseMenu();
                
                // Add a small delay to ensure menu close animation completes
                await Task.Delay(100);
                
                // Navigate to the logout page with forceLoad=true to ensure a full page refresh
                NavManager.NavigateTo("/Account/Logout", true);
            } catch (Exception ex) {
                // Log the error or handle it appropriately
                Console.WriteLine($"Error during logout: {ex.Message}");
                // Still try to navigate to logout even if menu close fails
                NavManager.NavigateTo("/Account/Logout", true);
            }
        }

        public void Dispose() {
            NavManager.LocationChanged -= OnLocationChanged;
        }
    }
}