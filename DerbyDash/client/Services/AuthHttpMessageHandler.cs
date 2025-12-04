using System.Net.Http.Headers;

namespace DerbyDash.Services {
    /// <summary>
    /// HTTP message handler that adds JWT token to API requests
    /// </summary>
    public class AuthHttpMessageHandler : DelegatingHandler {
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "authToken";

        public AuthHttpMessageHandler(ILocalStorageService localStorage) {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (!string.IsNullOrEmpty(token)) {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
