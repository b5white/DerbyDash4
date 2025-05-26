using Microsoft.AspNetCore.Components.Routing;

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

        // Handle logout directly from the NavMenu
        private void HandleLogout() {
            // Navigate to the logout page with forceLoad=true to ensure a full page refresh
            NavManager.NavigateTo("/Account/Logout", true);
        }

        public void Dispose() {
            NavManager.LocationChanged -= OnLocationChanged;
        }
    }
}