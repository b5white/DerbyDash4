using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider: ServerAuthenticationStateProvider {
        public void NotifyUserLogout() {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            base.NotifyAuthenticationStateChanged(authState);
        }
    }
}