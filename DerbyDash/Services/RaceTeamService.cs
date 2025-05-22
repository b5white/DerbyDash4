﻿﻿using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly AuthenticationStateProvider _authorizationState;
        private readonly ILogger<RaceTeamService> Logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJSRuntime _jsRuntime;
        private bool _triedLoadingFromCookie = false;


        private List<Racer> raceTeam = new() {
                new Racer { Id = 1, Name = "Alice", LastRaced = new DateOnly(2025, 2, 1) },
                new Racer { Id = 2, Name = "Bob", LastRaced = new DateOnly(2025, 3, 15) },
                new Racer { Id = 3, Name = "Charlie" }
            };
        private Racer? Active;
        private string? UserId;

        // Event that components can subscribe to for updates
        public event Action? OnRacerChanged;
        
        public Racer ActiveRacer {
            get => GetActiveRacer().GetAwaiter().GetResult();
            set => SetActiveRacer(value).Wait();
        }

        public RaceTeamService(
            AuthenticationStateProvider authorizationState,
            ILogger<RaceTeamService> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IJSRuntime jsRuntime) {
            _authorizationState = authorizationState;
            Logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _jsRuntime = jsRuntime;

            // Set a default active racer instead of trying to load from cookie during initialization
            // This avoids JS interop during prerendering
            Active = raceTeam.FirstOrDefault();
        }

        public async Task<List<Racer>> GetRacers() {
            Logger.LogInformation("GetRacers");
            raceTeam = await GetRacersInternal();
            return raceTeam;
        }

        public async Task<List<Racer>> GetRacersInternal() {
            Logger.LogInformation("GetRacersInternal");
            ApplicationUser? user;
            string userId;
            try {
                // Try to get the userId, but don't fail if we can't
                try {
                    userId = await GetUserID("GetRacers");
                    Logger.LogInformation($"Getting racers for user: {userId}");
                } catch (Exception ex) {
                    Logger.LogWarning(ex, "Could not get userId, but continuing");
                    // Continue even if we can't get the userId
                    return new List<Racer>();
                }

                user = await GetUserByIdAsync(userId);
                if (user == null) {
                    Logger.LogWarning("Could not get user, but continuing");
                    return new List<Racer>();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in GetRacers()");
                return new List<Racer>();
            }
            return await GetRacers(user);
        }

        public async Task<List<Racer>> GetRacers(ApplicationUser user) {
            Logger.LogInformation("GetRacers for ID: {ID}", user.Id);
            // Return the same data as GetRacers() for consistency
            await Task.CompletedTask; // Just to use 'await'
            return raceTeam;
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            Logger.LogInformation("GetRacerByIdAsync for ID: {ID}", racerId);
            await Task.CompletedTask; // Just to use 'await'
            return raceTeam.FirstOrDefault(r => r.Id == racerId);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId) {
            Logger.LogInformation("GetUserByIdAsync for userId: {userId}", userId);
            await Task.CompletedTask; // Just to use 'await'
            return new ApplicationUser() { UserName = name };
        }

        public async Task<Racer> AddRacer(Racer racer) {
            Logger.LogInformation("AddRacer for ID: {ID}", racer.Id);
            // Generate a unique ID if not provided
            if (racer.Id <= 0) {
                // Find the maximum ID and increment by 1
                int maxId = raceTeam.Count > 0 ? raceTeam.Max(r => r.Id) : 0;
                racer.Id = maxId + 1;
            }

            raceTeam.Add(racer);

            // Set as active racer if none is selected
            if (Active is null) {
                Active = racer;
            }

            // Notify subscribers that the racer list has changed
            OnRacerChanged?.Invoke();
            await Task.CompletedTask; // Just to use 'await'
            return racer;
        }

        public Task UpdateRacer(Racer racer) {
            // Implementation would go here
            return Task.CompletedTask;
        }

        public Task RemoveRacer(int racerId) {
            // Implementation would go here
            return Task.CompletedTask;
        }

        public async Task<Racer> GetActiveRacer() {
            Logger.LogInformation("GetActiveRacer");
            // Try to load from cookie if we haven't already attempted to do so
            if (!_triedLoadingFromCookie) {
                try {
                    await LoadActiveRacerFromCookieAsync();
                    _triedLoadingFromCookie = true;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error loading active racer, using default");
                    // If loading fails, keep the current Active value or set a default
                    if (Active == null) {
                        Active = raceTeam.FirstOrDefault();
                    }
                }
            }

            // If still null (which shouldn't happen), return the first racer
            if (Active == null) {
                Active = raceTeam.FirstOrDefault();
            }

            await Task.CompletedTask; // Just to use 'await'
            return Active!;
        }

        public async Task SetActiveRacer(Racer racer) {
            Logger.LogInformation("SetActiveRacer ID: {ID}", racer.Id);
            Active = racer;

            try {
                // Try to use JS interop, but catch the exception if we're prerendering
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
                    Logger.LogWarning("Could not get username for cookie, using 'guest' instead");
                }

                // Save the active racer ID in a user-specific cookie with 90-day expiration
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                await _jsRuntime.InvokeVoidAsync("setCookie", cookieName, racer.Id.ToString(), 90);
                Logger.LogInformation($"Saved active racer {racer.Name} (ID: {racer.Id}) to cookie for user {userIdentifier}");
            } catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time")) {
                // This is expected during prerendering, so just log at debug level
                Logger.LogDebug("Skipping cookie save during prerendering");
            } catch (Exception ex) {
                Logger.LogError(ex, $"Error saving active racer {racer.Name} (ID: {racer.Id}) to cookie");
            }

            // Notify subscribers that the active racer has changed
            OnRacerChanged?.Invoke();
        }

        // Load the active racer from cookie
        private async Task LoadActiveRacerFromCookieAsync() {
            Logger.LogInformation("LoadActiveRacerFromCookieAsync");
            try {
                // We'll try to use JS interop and catch any exceptions if we're prerendering

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
                    Logger.LogWarning("Could not get username for cookie, using 'guest' instead");
                }

                // Get the last active racer ID from the user-specific cookie
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                string? racerId = await _jsRuntime.InvokeAsync<string>("getCookie", cookieName);

                if (!string.IsNullOrEmpty(racerId) && int.TryParse(racerId, out int racerIdInt)) {
                    // Find the racer with the saved ID
                    Racer? racer = await GetRacerByIdAsync(racerIdInt);

                    if (racer != null) {
                        // Set as active racer without saving to cookie again
                        Active = racer;
                        Logger.LogInformation($"Loaded active racer {racer.Name} (ID: {racer.Id}) from cookie for user {userIdentifier}");

                        // Refresh the cookie with a new 90-day expiration
                        await _jsRuntime.InvokeVoidAsync("setCookie", cookieName, racerId, 90);
                    }
                } else {
                    // If no cookie found or racer not found, keep the current Active value
                    // or set the first racer as active if Active is null
                    if (Active == null) {
                        Active = raceTeam.FirstOrDefault();
                    }
                }
            } catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time")) {
                // This is expected during prerendering, so just log at debug level
                Logger.LogDebug("Skipping cookie load during prerendering");
                // Don't change Active here, keep whatever value it has
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading active racer from cookie");
                // Don't change Active here, keep whatever value it has
            }
        }

        public async Task<string> GetUserName(string purpose) {
            Logger.LogInformation("GetUserName for {purpose}", purpose);
            try {
                AuthenticationState authState = await _authorizationState.GetAuthenticationStateAsync();
                if (authState == null) {
                    Logger.LogError($"No authentication state found when trying to {purpose}.");
                    throw new MissingUserException();
                }
                string? userName = authState?.User?.Identity?.Name;
                if (string.IsNullOrEmpty(userName)) {
                    Logger.LogError($"Unable to determine the user name when trying to {purpose}.");
                    throw new MissingUserException();
                }
                return userName;
            } catch (Exception ex) {
                Logger.LogError(ex, $"Error getting username for {purpose}");
                throw new MissingUserException("Could not determine username", ex);
            }
        }

        /// <summary>
        /// Gets the last played race for the current active racer
        /// </summary>
        /// <returns>The identifier of the last played race, or null if not found</returns>
        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
               // Get the active racer
                 var activeRacer = await GetActiveRacer();
                
                if (activeRacer != null && !string.IsNullOrEmpty(activeRacer.LastPlayedRace)) {
                    _logger.LogInformation($"Retrieved last played race '{activeRacer.LastPlayedRace}' for racer {activeRacer.Name}");
                    return activeRacer.LastPlayedRace;
                }
                
                return null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error retrieving last played race for active racer");
                return null;
            }
        }

        /// <summary>
        /// Saves the last played race for the current active racer
        /// </summary>
        /// <param name="problemClassString">The identifier of the race (e.g., "addition-4stable")</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            try {
                // Get the active racer
                var activeRacer = await GetActiveRacer();
                
                if (activeRacer != null) {
                    // Update the LastPlayedRace property on the active racer
                    activeRacer.LastPlayedRace = problemClassString;
                    
                    // Also update the LastRaced date to today
                    activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Today);
                    
                    _logger.LogInformation($"Saved last played race '{problemClassString}' for racer {activeRacer.Name}");
                    
                    // Notify subscribers that the racer has been updated
                    OnRacerChanged?.Invoke();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error saving last played race for active racer");
            }
        }
        
        /// <summary>
        /// Increments the race count for the current active racer and the team total
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task IncrementRaceCountAsync() {
            try {
                // Get the current user
                var authState = await _authorizationState.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true) {
                    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId)) {
                        var appUser = await _userManager.FindByIdAsync(userId);
                        
                        if (appUser != null) {
                            // Increment the team race count
                            appUser.TeamRaceCount++;
                            
                            // Get the current active racer and increment their race count
                            var activeRacer = await GetActiveRacer();
                            if (activeRacer != null) {
                                activeRacer.RaceCount++;
                                
                                // Update the racer in the list
                                var racerIndex = raceTeam.FindIndex(r => r.Id == activeRacer.Id);
                                if (racerIndex >= 0) {
                                    raceTeam[racerIndex] = activeRacer;
                                }
                                
                                // Update the active racer reference
                                Active = activeRacer;
                                
                                _logger.LogInformation($"Incremented race count for racer {activeRacer.Name} to {activeRacer.RaceCount}");
                            }
                            
                            // Save changes to the database
                            await _userManager.UpdateAsync(appUser);
                            _logger.LogInformation($"Incremented team race count for user {userId} to {appUser.TeamRaceCount}");
                            
                            // Notify subscribers that the racer data has changed
                            OnRacerChanged?.Invoke();
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error incrementing race counts");
            }
        }
        
        /// <summary>
        /// Gets the total number of races completed by the current user's team
        /// </summary>
        /// <returns>The total number of races</returns>
        public async Task<int> GetTeamRaceCountAsync() {
            try {
                var authState = await _authorizationState.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user.Identity?.IsAuthenticated == true) {
                    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userId)) {
                        var appUser = await _userManager.FindByIdAsync(userId);

                        if (appUser != null) {
                            return appUser.TeamRaceCount;
                        }
                    }
                }
                
                return 0;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting team race count");
                return 0;
            }
        }
        
        /// <summary>
        /// Gets the number of races completed by the current active racer
        /// </summary>
        /// <returns>The number of races for the active racer</returns>
        public async Task<int> GetCurrentRacerRaceCountAsync() {
            try {
                var activeRacer = await GetActiveRacer();
                return activeRacer?.RaceCount ?? 0;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting current racer race count");
                return 0;
            }
        }
    }
}
