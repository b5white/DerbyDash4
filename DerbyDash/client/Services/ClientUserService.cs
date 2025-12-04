using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DerbyDash.Services {
    /// <summary>
    /// Client-side adapter that implements IUserService using AuthenticationStateProvider
    /// </summary>
    public class ClientUserService : IUserService {
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ApiAuthService _authService;
        private readonly ILogger<ClientUserService> _logger;
        private readonly ILocalStorageService _localStorage;

        private string? _cachedUserId;
        private bool _userIdCacheInitialized = false;

        public ClientUserService(
            AuthenticationStateProvider authStateProvider,
            ApiAuthService authService,
            ILogger<ClientUserService> logger,
            ILocalStorageService localStorage) {
            _authStateProvider = authStateProvider;
            _authService = authService;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<string?> GetUserIdAsync(string purpose = "", bool throwIfMissing = true) {
            var logContext = string.IsNullOrEmpty(purpose) ? "GetUserIdAsync" : $"GetUserIdAsync for {purpose}";
            _logger.LogDebug("{LogContext}", logContext);

            try {
                // Return cached value if available
                if (_userIdCacheInitialized && !string.IsNullOrEmpty(_cachedUserId)) {
                    return _cachedUserId;
                }

                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                if (authState?.User == null) {
                    if (throwIfMissing) {
                        _logger.LogDebug("No authentication state found when trying to {Purpose}", purpose);
                        throw new MissingUserException("No authentication state available");
                    }
                    return null;
                }

                var user = authState.User;
                if (user?.Identity == null || !user.Identity.IsAuthenticated) {
                    if (throwIfMissing) {
                        _logger.LogDebug("No authenticated user found when trying to {Purpose}", purpose);
                        throw new MissingUserException("User is not authenticated");
                    }
                    return null;
                }

                // Get user ID from JWT claim (NameIdentifier)
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId)) {
                    if (throwIfMissing) {
                        _logger.LogWarning("Unable to determine the user ID when trying to {Purpose}", purpose);
                        throw new MissingUserException("Could not determine user ID");
                    }
                    return null;
                }

                // Cache the user ID
                _cachedUserId = userId;
                _userIdCacheInitialized = true;

                _logger.LogDebug("Successfully retrieved user ID for {Purpose}", purpose);
                return userId;
            } catch (Exception ex) when (!(ex is MissingUserException)) {
                _logger.LogError(ex, "Error getting user ID for {Purpose}", purpose);
                if (throwIfMissing) {
                    throw new MissingUserException("Could not determine user ID", ex);
                }
                return null;
            }
        }

        public async Task<bool> IsLoggedInAsync() {
            try {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.Identity?.IsAuthenticated ?? false;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error checking if user is logged in");
                return false;
            }
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync() {
            try {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                if (authState?.User?.Identity?.IsAuthenticated != true) {
                    return null;
                }

                var user = authState.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = user.FindFirst(ClaimTypes.Email)?.Value;
                var userName = user.FindFirst(ClaimTypes.Name)?.Value;
                var emailConfirmed = user.FindFirst("email_verified")?.Value == "true";

                if (string.IsNullOrEmpty(userId)) {
                    return null;
                }

                return new ApplicationUser {
                    Id = userId,
                    Email = email,
                    UserName = userName,
                    EmailConfirmed = emailConfirmed
                };
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting current user");
                return null;
            }
        }

        public Task<ClaimsPrincipal?> GetCurrentPrincipalUserAsync() {
            try {
                // This is async but we can't await GetAuthenticationStateAsync in a sync method
                // So we return a task that gets the principal
                return GetAuthenticationStateAsync();
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting current user");
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        private async Task<ClaimsPrincipal?> GetAuthenticationStateAsync() {
            try {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.Identity?.IsAuthenticated == true ? authState.User : null;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting authentication state");
                return null;
            }
        }

        public async Task<string?> GetUserNameAsync() {
            try {
                var user = await GetAuthenticationStateAsync();
                return user?.Identity?.Name;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting user name");
                return null;
            }
        }

        public async Task<string?> GetUserClaimAsync(string claimType) {
            try {
                var user = await GetAuthenticationStateAsync();
                return user?.FindFirst(claimType)?.Value;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting user claim {ClaimType}", claimType);
                return null;
            }
        }

        public async Task<bool> IsInRoleAsync(string role) {
            try {
                var user = await GetAuthenticationStateAsync();
                return user?.IsInRole(role) ?? false;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error checking if user is in role {Role}", role);
                return false;
            }
        }

        public async Task<int> GetTotalUserCountAsync() {
            try {
                // This would require an API endpoint - for now return 0
                // TODO: Add API endpoint for user count
                return 0;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting total user count");
                return 0;
            }
        }
    }
}

