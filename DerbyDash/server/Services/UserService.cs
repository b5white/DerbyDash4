using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DerbyDash.Services {
    public class UserService: IUserService {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SessionData CurrentSession;
        private readonly ILogger<UserService> _logger;

        // Cache the user ID to avoid repeated lookups during a single request
        private string? _cachedUserId;
        private bool _userIdCacheInitialized = false;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            SessionData currentRequest,
            ILogger<UserService> logger
        ) {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            CurrentSession = currentRequest;
            _logger = logger;
        }

        public async Task<string?> GetUserIdAsync(string purpose = "", bool throwIfMissing = true) {
            var logContext = string.IsNullOrEmpty(purpose) ? "GetUserIdAsync" : $"GetUserIdAsync for {purpose}";
            _logger.LogDebug("{LogContext}", logContext);

            try {
                // Return cached value if available
                if (_userIdCacheInitialized && !string.IsNullOrEmpty(_cachedUserId)) {
                    return _cachedUserId;
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User == null) {
                    if (throwIfMissing) {
                        _logger.LogDebug("No HTTP context or user found when trying to {Purpose}", purpose);
                        throw new MissingUserException("No HTTP context available");
                    }
                    return null;
                }

                var user = httpContext.User;
                if (user?.Identity == null || !user.Identity.IsAuthenticated) {
                    if (throwIfMissing) {
                        _logger.LogDebug("No authenticated user found when trying to {Purpose}", purpose);
                        throw new MissingUserException("User is not authenticated");
                    }
                    return null;
                }

                // Get user ID from JWT claim (NameIdentifier) or UserManager
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? _userManager.GetUserId(user);
                
                if (string.IsNullOrEmpty(userId)) {
                    if (throwIfMissing) {
                        _logger.LogWarning("Unable to determine the user ID when trying to {Purpose}", purpose);
                        throw new MissingUserException("Could not determine user ID");
                    }
                    return null;
                }

                // Cache the user ID
                _cachedUserId = userId;
                CurrentSession.UserId = userId; // Update the current request context
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
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.User?.Identity?.IsAuthenticated ?? false;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error checking if user is logged in");
                return false;
            }
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync() {
            ClaimsPrincipal? userPrincipal = await GetCurrentPrincipalUserAsync();
            ApplicationUser? user = null;
            if (userPrincipal != null) {
                user = await _userManager.GetUserAsync(userPrincipal);
            }
            CurrentSession.UserId = user?.Id ?? ""; // Update the current request context with user ID
            return user;
        }

        public Task<ClaimsPrincipal?> GetCurrentPrincipalUserAsync() {
            try {
                var httpContext = _httpContextAccessor.HttpContext;
                var user = httpContext?.User?.Identity?.IsAuthenticated == true ? httpContext.User : null;
                return Task.FromResult(user);
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting current user");
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

        public async Task<string?> GetUserNameAsync() {
            try {
                var user = await GetCurrentPrincipalUserAsync();
                return user?.Identity?.Name;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting user name");
                return null;
            }
        }

        public async Task<string?> GetUserClaimAsync(string claimType) {
            try {
                var user = await GetCurrentPrincipalUserAsync();
                return user?.FindFirst(claimType)?.Value;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error getting user claim {ClaimType}", claimType);
                return null;
            }
        }

        public async Task<bool> IsInRoleAsync(string role) {
            try {
                var user = await GetCurrentPrincipalUserAsync();
                return user?.IsInRole(role) ?? false;
            } catch (Exception ex) {
                _logger.LogDebug(ex, "Error checking if user is in role {Role}", role);
                return false;
            }
        }

        public async Task<int> GetTotalUserCountAsync() {
            try {
                var users = await _userManager.Users.CountAsync();
                _logger.LogDebug("Retrieved total user count: {Count}", users);
                return users;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting total user count");
                return 0;
            }
        }
    }
}
