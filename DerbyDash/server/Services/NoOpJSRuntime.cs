using Microsoft.JSInterop;

namespace DerbyDash.Services {
    // No-op JSRuntime for server API (cookies are handled client-side)
    internal sealed class NoOpJSRuntime: IJSRuntime {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) {
            return ValueTask.FromResult<TValue>(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) {
            return ValueTask.FromResult<TValue>(default(TValue)!);
        }

        public ValueTask InvokeVoidAsync(string identifier, object?[]? args) {
            return ValueTask.CompletedTask;
        }

        public ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken, object?[]? args) {
            return ValueTask.CompletedTask;
        }
    }
}

