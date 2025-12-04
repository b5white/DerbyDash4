using Microsoft.JSInterop;

namespace DerbyDash.Services {
    /// <summary>
    /// Client-side localStorage service implementation using JavaScript interop
    /// </summary>
    public class LocalStorageService : ILocalStorageService {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }

        public async Task<T?> GetItemAsync<T>(string key) {
            try {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (string.IsNullOrEmpty(json)) {
                    return default(T);
                }
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            } catch (Exception) {
                return default(T);
            }
        }

        public async Task SetItemAsync<T>(string key, T value) {
            try {
                var json = System.Text.Json.JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            } catch (Exception) {
                // Ignore errors
            }
        }

        public async Task RemoveItemAsync(string key) {
            try {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            } catch (Exception) {
                // Ignore errors
            }
        }
    }
}
