using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        // This method is called when a user logs in or out to notify components of the state change
        public void NotifyUserAuthentication() {
            // For Blazor Server with Identity, the authentication state is automatically managed
            // We don't need to manually trigger state changes as the framework handles this
            // This method can be empty or we can trigger a general state change notification
        }
    }
}