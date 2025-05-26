using Microsoft.AspNetCore.Components.Server;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        // This method is called when a user logs out
        public void NotifyUserAuthentication() {
            var authStateTask = GetAuthenticationStateAsync();
            // Notify all components that the authentication state has changed
            // This will trigger the AuthenticationStateChanged event in all subscribed components
            NotifyAuthenticationStateChanged(authStateTask);
        }
    }
}