using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly IUserService _userService;
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

        public RaceTeamService(
            IUserService userService,
            ILogger<RaceTeamService> logger,
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IJSRuntime jsRuntime) {
            _userService = userService;
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
                // Check if user is authenticated first
                if (!await _userService.IsLoggedInAsync()) {
                    Logger.LogWarning("User is not authenticated when trying to GetRacers");
                    return new List<Racer>();
                }

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
            return new ApplicationUser() { UserName = "User_" + userId };
        }

        public async Task<Racer> AddRacer(Racer racer) {
            Logger.LogInformation("AddRacer for ID: {ID}", racer.Id);
            racer.UserId = await GetUserID("AddRacer");

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

        public async Task UpdateRacer(Racer racer) {
            Logger.LogInformation("UpdateRacer for ID: {ID}", racer.Id);
            await Task.CompletedTask; // Just to use 'await'
            return;
        }

        public async Task RemoveRacer(int racerId) {
            Logger.LogInformation("RemoveRacer for ID: {ID}", racerId);
            await Task.CompletedTask; // Just to use 'await'
            return;
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

            return Active!;
        }

        public async Task SetActiveRacer(Racer racer) {
            Logger.LogInformation("SetActiveRacer ID: {ID}", racer.Id);
            Active = racer;

            try {
                // Try to use JS interop, but catch the exception if we're prerendering
                // Get the current user's ID for the cookie name
                string userIdentifier = "guest";
                try {
                    var userId = await GetUserID("SetActiveRacer");
                    userIdentifier = userId.ToString();
                } catch (Exception) {
                    // If we can't get the userId, use "guest" as the identifier
                    Logger.LogWarning("Could not get userId for cookie, using guest instead");
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

                // Get the current user's ID for the cookie name
                string userIdentifier = "guest";
                try {
                    var userId = await GetUserID("LoadActiveRacer");
                    userIdentifier = userId.ToString();
                } catch (Exception) {
                    // If we can't get the userId, use "guest" as the identifier
                    Logger.LogWarning("Could not get userId for cookie, using 'guest' instead");
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

        public async Task<string> GetUserID(string purpose) {
            Logger.LogInformation("GetUserID for {purpose}", purpose);
            try {
                if (!string.IsNullOrEmpty(UserId)) {
                    return UserId;
                }

                // Check if user is authenticated first
                if (!await _userService.IsLoggedInAsync()) {
                    Logger.LogWarning("User is not authenticated when trying to {Purpose}", purpose);
                    throw new MissingUserException("User is not authenticated");
                }

                UserId = await _userService.GetUserIdAsync(purpose);
                return UserId;
            } catch (Exception ex) {
                Logger.LogError(ex, $"Error getting userId for {purpose}");
                throw new MissingUserException("Could not determine userId", ex);
            }
        }

        /// <summary>
        /// Gets the last played race for the current user from database
        /// </summary>
        /// <returns>The identifier of the last played race, or null if not found</returns>
        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
                // Get the current user
                string userId = await GetUserID("GetLastPlayedRaceAsync");
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && !string.IsNullOrEmpty(user.LastPlayedRace)) {
                    Logger.LogInformation($"Retrieved last played race '{user.LastPlayedRace}' for user {user.UserName}");
                    return user.LastPlayedRace;
                }
                return null;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error retrieving last played race for user");
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
                // Get the current user
                string userId = await GetUserID("SaveLastPlayedRaceAsync");
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null) {
                    user.LastPlayedRace = problemClassString;
                    await _userManager.UpdateAsync(user);
                    Logger.LogInformation($"Saved last played race '{problemClassString}' for user {user.UserName}");
                    OnRacerChanged?.Invoke();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error saving last played race for user");
            }
        }

        /// <summary>
        /// Gets the total number of races completed by the current user's team
        /// </summary>
        /// <returns>The total number of races</returns>
        public async Task<int> GetTeamRaceCountAsync() {
            try {
                string userId = await GetUserID("GetTeamRaceCountAsync");
                if (!string.IsNullOrEmpty(userId)) {
                    // Get the count for the current user
                    int count = 15; //await _context.Races
                                    //  .Where(r => _context.RaceTeam
                                    //      .Any(rt => rt.Id == r.RacerId && rt.UserId == userId))
                                    //  .CountAsync();
                    Logger.LogInformation("Count is {count}", count);
                    return count;
                }
                return 0;

            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting team race count");
                return 0;
            }
        }

        /// <summary>
        /// Gets the number of races completed by the current active racer
        /// </summary>
        /// <returns>The number of races for the active racer</returns>
        public async Task<int> GetCurrentRacerRaceCountAsync() {
            try {
                var activeRacer = await GetRacerWithRaceCountAsync();
                return activeRacer?.RaceCount ?? 0;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting current racer race count");
                return 0;
            }
        }

        public async Task<Racer?> GetRacerWithRaceCountAsync() {
            var racer = await GetActiveRacer();
            if (racer != null) {
                racer.RaceCount = 5;  //await _context.Races.CountAsync(r => r.FamilyMemberId == racerId);
            }
            return racer;
        }
    }
}
