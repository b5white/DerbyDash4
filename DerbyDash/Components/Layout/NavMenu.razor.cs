using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace DerbyDash.Components.Layout {
    public partial class NavMenu : IDisposable {
        private string? currentUrl;
        private AuthenticationState? authState;        protected override void OnInitialized() {
            currentUrl = NavManager.ToBaseRelativePath(NavManager.Uri);
            NavManager.LocationChanged += OnLocationChanged;
            
            // Subscribe to authentication state changes
            AuthStateProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
            currentUrl = NavManager.ToBaseRelativePath(e.Location);
            StateHasChanged();
        }

        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task) {
            // Update the authentication state when it changes
            authState = await task;
            
            // Force UI refresh
            await InvokeAsync(StateHasChanged);
        }

        // Handle logout directly from the NavMenu
        private void HandleLogout() {
            // Navigate to the logout page with forceLoad=true to ensure a full page refresh
            NavManager.NavigateTo("/Account/Logout", true);
        }

        public void Dispose() {
            NavManager.LocationChanged -= OnLocationChanged;
            AuthStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }
}