using Microsoft.AspNetCore.Components.Server;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        // This method is called when a user logs in or out to notify components of the state change
        public void NotifyUserAuthentication() {
            // For Blazor Server, we should not call GetAuthenticationStateAsync manually
            // Instead, just trigger a state change notification without getting the state
            // The framework will automatically refresh the state when components request it
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}