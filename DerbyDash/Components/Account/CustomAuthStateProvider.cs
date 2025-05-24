using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        // This method is called when a user logs out
        public void NotifyUserAuthentication() {
            // Create an anonymous user (not authenticated)
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // Notify all components that the authentication state has changed
            // This will trigger the AuthenticationStateChanged event in all subscribed components
            base.NotifyAuthenticationStateChanged(authState);
        }
    }
}