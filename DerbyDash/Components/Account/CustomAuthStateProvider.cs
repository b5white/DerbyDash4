using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        // This method is called when a user logs in or out to notify components of the state change
        public void NotifyUserLogin() {
            // Get the current authentication state (should be authenticated after sign-in)
            var currentAuthStateTask = GetAuthenticationStateAsync();

            // Notify all components that the authentication state has changed
            // This will trigger the AuthenticationStateChanged event in all subscribed components
            base.NotifyAuthenticationStateChanged(currentAuthStateTask);
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