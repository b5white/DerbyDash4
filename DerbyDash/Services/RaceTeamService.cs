using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.JSInterop;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly IUserService _userService;
        private readonly ILogger<RaceTeamService> Logger;
        private readonly SessionData CurrentSession;
        private readonly IJSRuntime _jsRuntime;

        private bool _triedLoadingFromCookie = false;


        private List<Racer> raceTeam = new() {
                new Racer { Id = 1, Name = "Alice", RaceCount = 5, LastRaced = new DateOnly(2025, 2, 1) },
                new Racer { Id = 2, Name = "Bob", RaceCount = 3, LastRaced = new DateOnly(2025, 3, 15) },
                new Racer { Id = 3, Name = "Charlie" }
            };
        private Racer? Active;
        private string? UserId;

        // Event that components can subscribe to for updates
        public event Func<Task>? OnRacerChanged;

        public RaceTeamService(
            IUserService userService,
            ILogger<RaceTeamService> logger,
            SessionData currentRequest,
            IJSRuntime jsRuntime) {
            _userService = userService;
            Logger = logger;
            CurrentSession = currentRequest;
            _jsRuntime = jsRuntime;
        }

        public async Task<List<Racer>> GetRacers() {
            Logger.LogInformation("GetRacers");
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
                    return new List<Racer>();
                }


            } catch (Exception ex) {
                Logger.LogError(ex, "Error in GetRacers()");
                return new List<Racer>();
            }
            return await GetRacersByUserId(userId);
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId) {
            Logger.LogInformation("GetRacersByUserId for ID: {ID}", userId);
            //List<Racer> racers = await _context.RaceTeam.Where(rt => rt.UserId == userId)
            //    .OrderBy(fm => fm.Name)
            //    .ToListAsync();
            //if (racers.Count == 0) {
            //    Logger.LogWarning("No Racers found for ID: {ID}", userId);
            //}
            await Task.CompletedTask; // Just to use 'await'
            return raceTeam;
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            Logger.LogInformation("GetRacerByIdAsync for ID: {ID}", racerId);
            Racer? racer = raceTeam.FirstOrDefault(r => r.Id == racerId);
            if (racer == null) {
                Logger.LogWarning("No Racer found for ID: {ID}", racerId);
            }
            await Task.CompletedTask; // Just to use 'await'
            return racer;
        }

        public async Task<Racer> AddRacer(Racer racer) {
            Logger.LogInformation("AddRacer for ID: {ID}", racer.Id);
            racer.UserId = await GetUserID("AddRacer");
            Logger.LogInformation($"Using UserId: {racer.UserId} for new racer");

            // Generate a unique ID if not provided
            if (racer.Id <= 0) {
                // Find the maximum ID and increment by 1
                int maxId = raceTeam.Count > 0 ? raceTeam.Max(r => r.Id) : 0;
                racer.Id = maxId + 1;
            }
            racer.RaceCount = 3;
            // await _context.RaceTeam.AddAsync(racer);
            // await _context.SaveChangesAsync();
            raceTeam.Add(racer);

            // Set as active racer only if this is the first racer for the user (when race team is being created)
            if (raceTeam.Count() == 1) {
                // Set as active racer if none is selected
                await SetActiveRacer(racer);
                Logger.LogInformation($"Set {racer.Name} as active racer (first racer for user)");
            }

            // Notify subscribers that the racer list has changed
            await InvokeOnRacerChanged();
            await Task.CompletedTask; // Just to use 'await'
            return racer;
        }

        public async Task UpdateRacer(Racer racer) {
            Logger.LogInformation("UpdateRacer for ID: {ID}", racer.Id);
            // _context.RaceTeam.Update(racer);
            // await _context.SaveChangesAsync();
            await Task.CompletedTask; // Just to use 'await'
            await InvokeOnRacerChanged();
        }

        public async Task RemoveRacer(int racerId) {
            Logger.LogInformation("RemoveRacer for ID: {ID}", racerId);
            await Task.CompletedTask; // Just to use 'await'
            await InvokeOnRacerChanged();
        }

        public async Task<Racer?> GetActiveRacer() {
            Logger.LogInformation("GetActiveRacer");
            if (CurrentSession.RacerId == 0) {
                Racer? active = null;
                // Try to load from cookie if we haven't already attempted to do so
                if (!_triedLoadingFromCookie) {
                    try {
                        active = await ReadActiveRacerFromCookieAsync();
                        _triedLoadingFromCookie = true;
                    } catch (Exception) {
                        Logger.LogWarning("Didn't load active racer from cookie");
                        // Don't default the active racer here - let it remain null
                    }
                }

                // Set as active racer only if this is the first racer for the user (when race team is being created)
                if (active is null) {
                    var racers = await GetRacers();
                    if (racers.Count == 1) {
                        active = racers[0];
                        Logger.LogInformation($"No active racer was set, defaulting to first racer: {active.Name} (ID: {active.Id})");
                        await SetActiveRacer(active);
                    }
                }
                if (active is not null) {
                    CurrentSession.RacerId = active.Id;
                }
                return active;
            } else {
                return await GetRacerByIdAsync(CurrentSession.RacerId);
            }
        }

        public async Task SetActiveRacer(Racer racer) {
            Logger.LogInformation("SetActiveRacer ID: {ID}", racer.Id);
            CurrentSession.RacerId = racer.Id;
            try {
                // Get the current user's ID for the cookie name
                string userIdentifier = "guest";
                try {
                    userIdentifier = await GetUserID("SetActiveRacer");
                } catch (Exception) {
                    // If we can't get the userId, use "guest" as the identifier
                    Logger.LogWarning("Could not get userId for cookie, using guest instead");
                }
                await WriteActiveRacerToCookieAsync(userIdentifier, racer.Id);

                // Notify subscribers that the active racer has changed
                await InvokeOnRacerChanged();
            } catch (Exception ex) {
                Logger.LogError(ex, $"Error saving active racer {racer.Name} (ID: {racer.Id}) to cookie");
            }
        }

        private async Task WriteActiveRacerToCookieAsync(string userIdentifier, int racerId) {
            try {
                // Try to use JS interop, but catch the exception if we're prerendering
                // Save the active racer ID in a user-specific cookie with 90-day expiration
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                await _jsRuntime.InvokeVoidAsync(
                    "setCookie",
                    cookieName,
                    racerId.ToString(),
                    90,           // expires in days
                    true,         // secure
                    "Lax",        // sameSite
                    "/"           // path
                );
                Logger.LogInformation($"Saved active racer ID: {racerId} to cookie for user {userIdentifier}");
            } catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time")) {
                // This is expected during prerendering, so just log at debug level
                Logger.LogDebug("Skipping cookie save during prerendering");
            }
        }

        // Read the active racer from cookie
        private async Task<Racer?> ReadActiveRacerFromCookieAsync() {
            Logger.LogInformation("ReadActiveRacerFromCookieAsync");
            Racer? racer = null;
            try {
                // We'll try to use JS interop and catch any exceptions if we're prerendering

                // Get the current user's ID for the cookie name
                string userIdentifier = "guest";
                try {
                    userIdentifier = await GetUserID("LoadActiveRacer");
                } catch (Exception) {
                    // If we can't get the userId, use "guest" as the identifier
                    Logger.LogWarning("Could not get userId for cookie, using 'guest' instead");
                }

                // Get the last active racer ID from the user-specific cookie
                string cookieName = $"lastActiveRacer_{userIdentifier}";
                string? racerId = await _jsRuntime.InvokeAsync<string>("getCookie", cookieName);

                if (!string.IsNullOrEmpty(racerId) && int.TryParse(racerId, out int racerIdInt)) {
                    // Find the racer with the saved ID
                    racer = await GetRacerByIdAsync(racerIdInt);

                    if (racer != null) {
                        Logger.LogInformation($"Loaded active racer {racer.Name} (ID: {racer.Id}) from cookie for user {userIdentifier}");

                        // Refresh the cookie with a new 90-day expiration
                        await WriteActiveRacerToCookieAsync(userIdentifier, racerIdInt);
                    }
                } else {
                    // If no cookie found or racer not found, keep the current Active value
                    // Don't default to first racer - only set active racer when explicitly loaded from cookie
                    Logger.LogDebug("No active racer cookie found or racer no longer exists");
                }
            } catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop calls cannot be issued at this time")) {
                // This is expected during prerendering, so just log at debug level
                Logger.LogDebug("Skipping cookie load during prerendering");
                // Don't change Active here, keep whatever value it has
            } catch (Exception ex) {
                Logger.LogError(ex, "Error loading active racer from cookie");
                // Don't change Active here, keep whatever value it has
            }
            return racer;
        }

        private async Task<string> GetUserID(string purpose) {
            Logger.LogInformation("GetUserID for {purpose}", purpose);
            try {
                if (!string.IsNullOrEmpty(CurrentSession.UserId)) {
                    return CurrentSession.UserId;
                }

                // Check if user is authenticated first
                if (!await _userService.IsLoggedInAsync()) {
                    Logger.LogWarning("User is not authenticated when trying to {Purpose}", purpose);
                    throw new MissingUserException("User is not authenticated");
                }

                string UserId = await _userService.GetUserIdAsync(purpose);
                CurrentSession.UserId = UserId;
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
                // Get the active racer
                var activeRacer = await GetActiveRacer();

                if (activeRacer != null && !string.IsNullOrEmpty(activeRacer.LastPlayedRace)) {
                    Logger.LogInformation($"Retrieved last played race '{activeRacer.LastPlayedRace}' for racer {activeRacer.Name}");
                    return activeRacer.LastPlayedRace;
                }
                return null;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error retrieving last played race for active racer");
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
                    // Update the racer's last played race and last raced date

                    activeRacer.LastPlayedRace = problemClassString;
                    activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Today);
                    Logger.LogInformation($"Saved last played race '{problemClassString}' for racer {activeRacer.Name}");
                    // await context.SaveChangesAsync();

                    // Notify subscribers that the racer has been updated
                    await InvokeOnRacerChanged();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error saving last played race {ProblemClass}", problemClassString);
                throw;
            }
        }

        /// <summary>
        /// Gets the total number of races completed by the current user's team
        /// </summary>
        /// <returns>The total number of races</returns>
        public async Task<int> GetTeamRaceCountAsync() {
            try {
                //string userId = await GetUserID("GetTeamRaceCountAsync");
                //if (!string.IsNullOrEmpty(userId)) {
                int count = 15; //await context.Races
                                //        .Where(r => context.Racers.Any(racer => racer.UserId == userId && racer.Id == r.RacerId))
                                //        .CountAsync();

                //    Logger.LogInformation("Team race count for user {UserId}: {Count}", userId, count);
                await Task.CompletedTask; // Just to use 'await'
                return count;
                // }
                //Logger.LogWarning("GetTeamRaceCountAsync: UserId is null or empty");
                //return 0;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting team race count");
                return 0; // Return 0 instead of throwing to make UI more resilient
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
                //??
                // Get race count using a separate query to avoid concurrency issues
                //using var context = _contextFactory.CreateDbContext();
                //var raceCount = await context.Races.CountAsync(r => r.RacerId == racer.Id);
                //racer.RaceCount = raceCount;
            }
            return racer;
        }

        private async Task InvokeOnRacerChanged() {
            if (OnRacerChanged != null) {
                var handlers = OnRacerChanged.GetInvocationList().Cast<Func<Task>>();
                foreach (var handler in handlers) {
                    await handler(); // Await each handler
                }
            }
        }

        /// <summary>
        /// Maps problem class strings to integer IDs for database storage
        /// </summary>
        /// <param name="problemClassString">The problem class string (e.g., "addition-4stable")</param>
        /// <returns>Integer ID for the problem set</returns>
        private int GetProblemSetId(string problemClassString) {
            return problemClassString.ToLowerInvariant() switch {
                "addition-4stable" => 1,
                "subtraction-4stable" => 2,
                "multiplication-4stable" => 3,
                "division-4stable" => 4,
                "addition-unstable" => 5,
                "subtraction-unstable" => 6,
                "multiplication-unstable" => 7,
                "division-unstable" => 8,
                "mixed-4stable" => 9,
                "mixed-unstable" => 10,
                _ => 999 // Unknown problem type
            };
        }
    }
}
