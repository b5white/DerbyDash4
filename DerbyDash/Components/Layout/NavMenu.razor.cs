using Microsoft.AspNetCore.Components.Routing;

namespace DerbyDash.Components.Layout {
    public partial class NavMenu {
        private string? currentUrl;

        protected override void OnInitialized() {
            currentUrl = NavManager.ToBaseRelativePath(NavManager.Uri);
            NavManager.LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
            currentUrl = NavManager.ToBaseRelativePath(e.Location);
            StateHasChanged();
        }

        public void Dispose() {
            NavManager.LocationChanged -= OnLocationChanged;
        }
    }
}