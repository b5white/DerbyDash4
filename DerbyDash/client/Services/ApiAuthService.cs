using DerbyDash.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace DerbyDash.Services {
    public class ApiAuthService {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";

        public ApiAuthService(HttpClient httpClient, ILocalStorageService localStorage) {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<LoginResponse>?> LoginAsync(string email, string password, string? recaptchaToken = null) {
            var request = new LoginRequest {
                Email = email,
                Password = password,
                RecaptchaToken = recaptchaToken
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                if (result?.Success == true && result.Data != null) {
                    await _localStorage.SetItemAsync(TokenKey, result.Data.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, result.Data.RefreshToken);
                }
                return result;
            } else {
                // Read error response
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                return errorResult;
            }
        }

        public async Task<ApiResponse<LoginResponse>?> RegisterAsync(string email, string password, string confirmPassword, string? recaptchaToken = null) {
            var request = new RegisterRequest {
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword,
                RecaptchaToken = recaptchaToken
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                if (result?.Success == true && result.Data != null) {
                    await _localStorage.SetItemAsync(TokenKey, result.Data.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, result.Data.RefreshToken);
                }
                return result;
            } else {
                // Read error response
                var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
                return errorResult;
            }
        }

        public async Task LogoutAsync() {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
            if (!string.IsNullOrEmpty(refreshToken)) {
                var request = new LogoutRequest {
                    RefreshToken = refreshToken
                };
                await _httpClient.PostAsJsonAsync("/api/auth/logout", request);
            }
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
        }

        public async Task<string?> GetTokenAsync() {
            return await _localStorage.GetItemAsync<string>(TokenKey);
        }

        public async Task SetTokenAsync(string token) {
            await _localStorage.SetItemAsync(TokenKey, token);
        }

        public async Task<ApiResponse<RefreshTokenResponse>?> RefreshTokenAsync(string accessToken, string refreshToken) {
            var request = new RefreshTokenRequest {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/refresh", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RefreshTokenResponse>>();
                if (result?.Success == true && result.Data != null) {
                    await _localStorage.SetItemAsync(TokenKey, result.Data.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, result.Data.RefreshToken);
                }
                return result;
            }
            return null;
        }

        public async Task InitializeAuthAsync() {
            // Token will be added by AuthHttpMessageHandler automatically
            await Task.CompletedTask;
        }
    }
}

