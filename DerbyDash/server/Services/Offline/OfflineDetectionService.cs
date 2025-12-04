using Microsoft.JSInterop;

namespace DerbyDash.Services.Offline {
    public interface IOfflineDetectionService {
        Task<bool> IsOfflineAsync();
        event Func<bool, Task>? OnOfflineStatusChanged;
    }

    public class OfflineDetectionService : IOfflineDetectionService, IAsyncDisposable {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<OfflineDetectionService> _logger;
        private DotNetObjectReference<OfflineDetectionService>? _objRef;
        private bool _isOffline = false;

        public event Func<bool, Task>? OnOfflineStatusChanged;

        public OfflineDetectionService(IJSRuntime jsRuntime, ILogger<OfflineDetectionService> logger) {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task<bool> IsOfflineAsync() {
            try {
                // Check if we're in a browser environment
                var isOnline = await _jsRuntime.InvokeAsync<bool>("navigator.onLine");
                _isOffline = !isOnline;
                return _isOffline;
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Could not detect online status, assuming online");
                return false;
            }
        }

        public async Task InitializeAsync() {
            try {
                _objRef = DotNetObjectReference.Create(this);
                
                // Set up event listeners for online/offline events
                await _jsRuntime.InvokeVoidAsync("window.setupOfflineDetection", _objRef);
                
                // Get initial status
                _isOffline = await IsOfflineAsync();
                _logger.LogInformation("Offline detection initialized. Currently {Status}", _isOffline ? "offline" : "online");
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to initialize offline detection");
            }
        }

        [JSInvokable]
        public async Task OnOfflineStatusChange(bool isOffline) {
            _logger.LogInformation("Network status changed to: {Status}", isOffline ? "offline" : "online");
            _isOffline = isOffline;
            
            if (OnOfflineStatusChanged != null) {
                await OnOfflineStatusChanged.Invoke(isOffline);
            }
        }

        public async ValueTask DisposeAsync() {
            if (_objRef != null) {
                try {
                    await _jsRuntime.InvokeVoidAsync("window.cleanupOfflineDetection");
                } catch (Exception ex) {
                    _logger.LogWarning(ex, "Error during offline detection cleanup");
                }
                _objRef.Dispose();
            }
        }
    }
}
