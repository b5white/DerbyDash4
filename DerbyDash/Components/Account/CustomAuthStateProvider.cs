using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DerbyDash.Components.Account {

    public class CustomAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        SessionData CurrentSession,
        UserManager<ApplicationUser> UserManager): ServerAuthenticationStateProvider {

        // This method is called when a user logs in or out to notify components of the state change
        public void NotifyUserLogin() {
            // Get the current user (should be authenticated after sign-in)
            ClaimsPrincipal user = GetCurrentUser();
            CurrentSession.UserId = UserManager.GetUserId(user) ?? "";
            // Create a new authentication state with the authenticated user
            var authState = Task.FromResult(new AuthenticationState(user));

            // Notify all components that the authentication state has changed
            // This will trigger the AuthenticationStateChanged event in all subscribed components
            base.NotifyAuthenticationStateChanged(authState);
        }

        // This method is called when a user logs out
        public void NotifyUserLogout() {
            // Create an anonymous user (not authenticated)
            CurrentSession.UserId = ""; // Clear the user ID in the session context
            CurrentSession.RacerId = 0; // Clear the current racer ID in the session context
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            // Notify all components that the authentication state has changed
            base.NotifyAuthenticationStateChanged(authState);
        }

        public ClaimsPrincipal GetCurrentUser() {
            // Get the ClaimsPrincipal from the HttpContext
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal == null)
                throw new InvalidOperationException("No authenticated user.");

            return principal;
        }

    }
}