using DerbyDash.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DerbyDash.Components.Account {
    public class CustomAuthStateProvider : AuthenticationStateProvider {
        private readonly ILocalStorageService _localStorage;
        private readonly ApiAuthService _authService;
        private const string TokenKey = "authToken";

        public CustomAuthStateProvider(ILocalStorageService localStorage, ApiAuthService authService) {
            _localStorage = localStorage;
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            
            if (string.IsNullOrEmpty(token)) {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                var claims = jsonToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                
                return new AuthenticationState(user);
            } catch {
                // Invalid token, return anonymous
                await _localStorage.RemoveItemAsync(TokenKey);
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public void NotifyUserLogin() {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserAuthentication(string token) {
            try {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                var claims = jsonToken.Claims.ToList();
                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            } catch {
                // Invalid token, return anonymous
                NotifyAuthenticationStateChanged(Task.FromResult(
                    new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
            }
        }

        public void NotifyUserLogout() {
            NotifyAuthenticationStateChanged(Task.FromResult(
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }
    }
}
