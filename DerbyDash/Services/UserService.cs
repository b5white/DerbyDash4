using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Utilities.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DerbyDash.Services {
    public class UserService: IUserService {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CurrentRequestDTO CurrentRequest;
        private readonly ILogger<UserService> _logger;

        // Cache the user ID to avoid repeated lookups during a single request
        private string? _cachedUserId;
        private bool _userIdCacheInitialized = false;

        public UserService(
            AuthenticationStateProvider authenticationStateProvider,
            UserManager<ApplicationUser> userManager,
            CurrentRequestDTO currentRequest,
            ILogger<UserService> logger
        ) {
            _authenticationStateProvider = authenticationStateProvider;
            _userManager = userManager;
            CurrentRequest = currentRequest;
            _logger = logger;
        }

        public async Task<string> GetUserIdAsync(string purpose = "") {
            var logContext = string.IsNullOrEmpty(purpose) ? "GetUserIdAsync" : $"GetUserIdAsync for {purpose}";
            _logger.LogInformation("{LogContext}", logContext);

            try {
                // Return cached value if available
                if (_userIdCacheInitialized && !string.IsNullOrEmpty(_cachedUserId)) {
                    return _cachedUserId;
                }

                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                if (authState?.User == null) {
                    _logger.LogWarning("No authentication state found when trying to {Purpose}", purpose);
                    throw new MissingUserException("No authentication state available");
                }

                var user = authState.User;
                if (user?.Identity == null || !user.Identity.IsAuthenticated) {
                    _logger.LogWarning("No authenticated user found when trying to {Purpose}", purpose);
                    throw new MissingUserException("User is not authenticated");
                }

                var userId = _userManager.GetUserId(user);
                if (string.IsNullOrEmpty(userId)) {
                    _logger.LogWarning("Unable to determine the user ID when trying to {Purpose}", purpose);
                    throw new MissingUserException("Could not determine user ID");
                }

                // Cache the user ID
                _cachedUserId = userId;
                CurrentRequest.UserId = userId; // Update the current request context
                _userIdCacheInitialized = true;

                _logger.LogInformation("Successfully retrieved user ID for {Purpose}", purpose);
                return userId;
            } catch (Exception ex) when (!(ex is MissingUserException)) {
                _logger.LogError(ex, "Error getting user ID for {Purpose}", purpose);
                throw new MissingUserException("Could not determine user ID", ex);
            }
        }

        public async Task<bool> IsLoggedInAsync() {
            try {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.Identity?.IsAuthenticated ?? false;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error checking if user is logged in");
                return false;
            }
        }

        public async Task<ClaimsPrincipal?> GetCurrentUserAsync() {
            try {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                return authState?.User?.Identity?.IsAuthenticated == true ? authState.User : null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<string?> GetUserNameAsync() {
            try {
                var user = await GetCurrentUserAsync();
                return user?.Identity?.Name;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting user name");
                return null;
            }
        }

        public async Task<string?> GetUserClaimAsync(string claimType) {
            try {
                var user = await GetCurrentUserAsync();
                return user?.FindFirst(claimType)?.Value;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting user claim {ClaimType}", claimType);
                return null;
            }
        }

        public async Task<bool> IsInRoleAsync(string role) {
            try {
                var user = await GetCurrentUserAsync();
                return user?.IsInRole(role) ?? false;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error checking if user is in role {Role}", role);
                return false;
            }
        }
    }
}
