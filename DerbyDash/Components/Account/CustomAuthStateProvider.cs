using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider : ServerAuthenticationStateProvider {
        // This method is called when a user logs in to notify components of the state change
        public void NotifyUserAuthentication() {
            // For Blazor Server, we should not call GetAuthenticationStateAsync manually
            // Instead, just trigger a state change notification without getting the state
            // The framework will automatically refresh the state when components request it
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
        }
        
        // This method is called when a user logs out
        public void NotifyUserLogout() {
            // Create an anonymous user (not authenticated)
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // Notify all components that the authentication state has changed
            base.NotifyAuthenticationStateChanged(authState);
        }
    }
}