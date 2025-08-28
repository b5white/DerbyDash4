using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DerbyDash.Services {
    public interface IApiAuthService {
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetTokenAsync();
        Task SetTokenAsync(string token, string refreshToken);
        Task ClearTokenAsync();
        Task<HttpClient> GetAuthenticatedHttpClientAsync();
        event Action<bool> AuthenticationStateChanged;
    }

    public class ApiAuthService : IApiAuthService {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiAuthService> _logger;
        private string? _currentToken;
        private string? _currentRefreshToken;

        public event Action<bool>? AuthenticationStateChanged;

        public ApiAuthService(IJSRuntime jsRuntime, HttpClient httpClient, ILogger<ApiAuthService> logger) {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> IsAuthenticatedAsync() {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string?> GetTokenAsync() {
            if (_currentToken != null) {
                return _currentToken;
            }

            try {
                _currentToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "jwtToken");
                return _currentToken;
            } catch {
                return null;
            }
        }

        public async Task SetTokenAsync(string token, string refreshToken) {
            _currentToken = token;
            _currentRefreshToken = refreshToken;

            try {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
                AuthenticationStateChanged?.Invoke(true);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error storing tokens");
            }
        }

        public async Task ClearTokenAsync() {
            _currentToken = null;
            _currentRefreshToken = null;

            try {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
                AuthenticationStateChanged?.Invoke(false);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error clearing tokens");
            }
        }

        public async Task<HttpClient> GetAuthenticatedHttpClientAsync() {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token)) {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return _httpClient;
        }
    }

    // Extension methods for HttpClient to make authenticated API calls easier
    public static class HttpClientExtensions {
        public static async Task<HttpResponseMessage> GetWithAuthAsync(this HttpClient client, string requestUri, string? token = null) {
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await client.GetAsync(requestUri);
        }

        public static async Task<HttpResponseMessage> PostWithAuthAsync<T>(this HttpClient client, string requestUri, T value, string? token = null) {
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await client.PostAsJsonAsync(requestUri, value);
        }

        public static async Task<HttpResponseMessage> PutWithAuthAsync<T>(this HttpClient client, string requestUri, T value, string? token = null) {
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await client.PutAsJsonAsync(requestUri, value);
        }

        public static async Task<HttpResponseMessage> DeleteWithAuthAsync(this HttpClient client, string requestUri, string? token = null) {
            if (!string.IsNullOrEmpty(token)) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await client.DeleteAsync(requestUri);
        }
    }
}
