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
            await JSRuntime.InvokeVoidAsync("removeClass", navScrollableElement, "show-menu");
        }

        // Handle logout directly from the NavMenu
        private async void HandleLogout() {
            try {
                // Close the mobile menu first
                await CloseMenu();
                // Navigate to the logout page with forceLoad=true to ensure a full page refresh
                NavManager.NavigateTo("/Account/Logout", true);
            } catch (Exception ex) {
            }
        }

        public void Dispose() {
            NavManager.LocationChanged -= OnLocationChanged;
        }
    }
}