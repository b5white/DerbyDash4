using Microsoft.AspNetCore.Components;

namespace DerbyDash.Components.Account {
    /// <summary>
    /// Client-side redirect manager that uses NavigationManager
    /// </summary>
    internal sealed class IdentityRedirectManager {
        private readonly NavigationManager _navManager;
        private readonly ILogger<IdentityRedirectManager>? _logger;

        public IdentityRedirectManager(NavigationManager navManager, ILogger<IdentityRedirectManager>? logger = null) {
            _navManager = navManager;
            _logger = logger;
        }

        public void RedirectTo(string? uri, bool forceLoad = false) {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative)) {
                uri = _navManager.ToBaseRelativePath(uri);
            }
            
            _logger?.LogInformation($"Redirecting to {uri}");
            _navManager.NavigateTo(uri, forceLoad);
        }

        public void RedirectToCurrentPage() {
            var currentPath = _navManager.ToAbsoluteUri(_navManager.Uri).GetLeftPart(UriPartial.Path);
            RedirectTo(currentPath);
        }
    }
}
