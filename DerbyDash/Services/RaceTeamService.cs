﻿using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly AuthenticationStateProvider _authorizationState;
        private readonly ILogger<RaceTeamService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJSRuntime _jsRuntime;
        private List<Racer> raceTeam = new() {
                new Racer { Id = "1", Name = "Alice", LastRaced = new DateOnly(2025, 2, 1) },
                new Racer { Id = "2", Name = "Bob", LastRaced = new DateOnly(2025, 3, 15) },
                new Racer { Id = "3", Name = "Charlie" }
            };
        private Racer? Active;

        // Event that components can subscribe to for updates
        public event Action? OnRacerChanged;

        public RaceTeamService(
            AuthenticationStateProvider authorizationState,
            ILogger<RaceTeamService> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IJSRuntime jsRuntime) {
            _authorizationState = authorizationState;
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _jsRuntime = jsRuntime;

            // Load the active racer from cookie on initialization
            LoadActiveRacerFromCookieAsync().ConfigureAwait(false);
        }

        public async Task<List<Racer>> GetRacers() {
            ApplicationUser user;
            string name;
            try {
                // Try to get the username, but don't fail if we can't
                try {
                    name = await GetUserName("GetRacers");
                    _logger.LogInformation($"Getting racers for user: {name}");
                } catch (Exception ex) {
                    _logger.LogWarning(ex, "Could not get username, but continuing");
                    // Continue even if we can't get the username
                    return new List<Racer>();
                }

                user = await GetUserByNameAsync(name);
                if (user == null) {
                    return new List<Racer>();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error in GetRacers()");
                return new List<Racer>();
            }
            return await GetRacers(user);
        }

        public async Task<List<Racer>> GetRacers(ApplicationUser user) {
            // Return the same data as GetRacers() for consistency
            return raceTeam;
        }

        public async Task<Racer?> GetRacerByIdAsync(string racerId) {
            return raceTeam.FirstOrDefault(r => r.Id == racerId);
        }

        public async Task<ApplicationUser?> GetUserByNameAsync(string name) {
            return new ApplicationUser() { UserName = name };
        }

        public async Task<Racer> AddRacer(Racer racer) {
            // Generate a unique ID if not provided
            if (string.IsNullOrEmpty(racer.Id)) {
                racer.Id = Guid.NewGuid().ToString();
            }

            raceTeam.Add(racer);

            // Set as active racer if none is selected
            if (Active is null) {
                Active = racer;
            }

            // Notify subscribers that the racer list has changed
            OnRacerChanged?.Invoke();

            return racer;
        }

        public async Task UpdateRacer(Racer racer) {

        }

        public async Task RemoveRacer(string racerId) {

        }

        public Racer ActiveRacer {
            get => GetActiveRacer().GetAwaiter().GetResult();
            set => SetActiveRacer(value).Wait();
        }

        public async Task<Racer?> GetActiveRacer() {
            // If no active racer is set, try to load it from cookie
            if (Active == null) {
                await LoadActiveRacerFromCookieAsync();
            }

            return Active;
        }

        public async Task SetActiveRacer(Racer racer) {
            Active = racer;

            try {
                // Get the current user's ID or email for the cookie name
                string userIdentifier = "guest";
                try {
                    var userName = await GetUserName("SetActiveRacer");
                    if (!string.IsNullOrEmpty(userName)) {
                        // Use a hash or sanitized version of the email/username to avoid special characters in cookie name
                        userIdentifier = userName.Replace("@", "_at_").Replace(".", "_dot_");
                    }
                } catch (Exception) {
                    // If we can't get the username, use "guest" as the identifier
                    _logger.LogWarning("Could not get username for cookie, using 'guest' instead");
                }

                // Save the active racer ID in a user-specific cookie with 90-day expiration
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                await _jsRuntime.InvokeVoidAsync("setCookie", cookieName, racer.Id, 90);
                _logger.LogInformation($"Saved active racer {racer.Name} (ID: {racer.Id}) to cookie for user {userIdentifier}");
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error saving active racer {racer.Name} (ID: {racer.Id}) to cookie");
            }

            // Notify subscribers that the active racer has changed
            OnRacerChanged?.Invoke();
        }

        // Load the active racer from cookie
        private async Task LoadActiveRacerFromCookieAsync() {
            try {
                // Get the current user's ID or email for the cookie name
                string userIdentifier = "guest";
                try {
                    var userName = await GetUserName("LoadActiveRacer");
                    if (!string.IsNullOrEmpty(userName)) {
                        // Use a hash or sanitized version of the email/username to avoid special characters in cookie name
                        userIdentifier = userName.Replace("@", "_at_").Replace(".", "_dot_");
                    }
                } catch (Exception) {
                    // If we can't get the username, use "guest" as the identifier
                    _logger.LogWarning("Could not get username for cookie, using 'guest' instead");
                }

                // Get the last active racer ID from the user-specific cookie
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                string? racerId = await _jsRuntime.InvokeAsync<string>("getCookie", cookieName);

                if (!string.IsNullOrEmpty(racerId)) {
                    // Find the racer with the saved ID
                    Racer? racer = await GetRacerByIdAsync(racerId);

                    if (racer != null) {
                        // Set as active racer without saving to cookie again
                        Active = racer;
                        _logger.LogInformation($"Loaded active racer {racer.Name} (ID: {racer.Id}) from cookie for user {userIdentifier}");

                        // Refresh the cookie with a new 90-day expiration
                        await _jsRuntime.InvokeVoidAsync("setCookie", cookieName, racerId, 90);
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error loading active racer from cookie");
            }
        }

        public async Task<string> GetUserName(string purpose) {
            try {
                AuthenticationState authState = await _authorizationState.GetAuthenticationStateAsync();
                if (authState == null) {
                    _logger.LogError($"No authentication state found when trying to {purpose}.");
                    throw new MissingUserException();
                }
                string? userName = authState?.User?.Identity?.Name;
                if (userName == null) {
                    _logger.LogError($"Unable to determine the user name when trying to {purpose}.");
                    throw new MissingUserException();
                }
                return userName;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting username for {purpose}");
                throw new MissingUserException("Could not determine username", ex);
            }
        }
    }
}
