using DerbyDash.Data;
using DerbyDash.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DerbyDash.Services {
    public class RaceTeamService: IRaceTeamService {
        private readonly IUserService _userService;
        private readonly ILogger<RaceTeamService> Logger;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJSRuntime _jsRuntime;
        private readonly IServiceProvider _serviceProvider;
        private bool _triedLoadingFromCookie = false;


        private List<Racer> raceTeam = new();
        private Racer? Active;
        private string? UserId;

        // Event that components can subscribe to for updates
        public event Action? OnRacerChanged;

        public RaceTeamService(
            IUserService userService,
            ILogger<RaceTeamService> logger,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IJSRuntime jsRuntime,
            IServiceProvider serviceProvider) {
            _userService = userService;
            Logger = logger;
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _jsRuntime = jsRuntime;
            _serviceProvider = serviceProvider;

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
            try {
                // Check if user is authenticated first
                if (!await _userService.IsLoggedInAsync()) {
                    Logger.LogWarning("User is not authenticated when trying to GetRacers");
                    return new List<Racer>();
                }

                // Try to get the userId
                string userId;
                try {
                    userId = await GetUserID("GetRacers");
                    Logger.LogInformation($"Getting racers for user: {userId}");
                } catch (Exception ex) {
                    Logger.LogWarning(ex, "Could not get userId, but continuing");
                    return new List<Racer>();
                }

                // Load racers from database with race counts using a single query
                using var context = _contextFactory.CreateDbContext();
                var racers = await context.Racers
                    .Where(r => r.UserId == userId)
                    .Select(r => new Racer {
                        Id = r.Id,
                        Name = r.Name,
                        UserId = r.UserId,
                        LastRaced = r.LastRaced,
                        LastPlayedRace = r.LastPlayedRace,
                        AvatarFileName = r.AvatarFileName,
                        RaceCount = context.Races.Count(race => race.RacerId == r.Id)
                    })
                    .ToListAsync();

                Logger.LogInformation($"Loaded {racers.Count} racers from database");
                return racers;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error in GetRacersInternal()");
                return new List<Racer>();
            }
        }

        public async Task<List<Racer>> GetRacers(ApplicationUser user) {
            Logger.LogInformation("GetRacers for ID: {ID}", user.Id);
            
            // Load racers from database with race counts
            using var context = _contextFactory.CreateDbContext();
            var racers = await context.Racers
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            // Calculate race count for each racer
            foreach (var racer in racers) {
                racer.RaceCount = await context.Races
                    .CountAsync(r => r.RacerId == racer.Id);
            }

            return racers;
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            Logger.LogInformation("GetRacerByIdAsync for ID: {ID}", racerId);
            
            try {
                using var context = _contextFactory.CreateDbContext();
                var racer = await context.Racers
                    .Where(r => r.Id == racerId)
                    .Select(r => new Racer {
                        Id = r.Id,
                        Name = r.Name,
                        UserId = r.UserId,
                        LastRaced = r.LastRaced,
                        LastPlayedRace = r.LastPlayedRace,
                        AvatarFileName = r.AvatarFileName,
                        RaceCount = context.Races.Count(race => race.RacerId == r.Id)
                    })
                    .FirstOrDefaultAsync();
                
                return racer;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting racer by ID {RacerId}", racerId);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId) {
            Logger.LogInformation("GetUserByIdAsync for userId: {userId}", userId);
            await Task.CompletedTask; // Just to use 'await'
            return new ApplicationUser() { UserName = "User_" + userId };
        }

        public async Task<Racer> AddRacer(Racer racer) {
            Logger.LogInformation("AddRacer for Name: {Name}", racer.Name);
            
            try {
                racer.UserId = await GetUserID("AddRacer");

                // Check for duplicate names for this user
                using var context = _contextFactory.CreateDbContext();
                var existingRacer = await context.Racers
                    .FirstOrDefaultAsync(r => r.UserId == racer.UserId && r.Name == racer.Name);
                
                if (existingRacer != null) {
                    throw new DuplicateRacerException($"Racer with name '{racer.Name}' already exists");
                }

                // Add to database
                context.Racers.Add(racer);
                await context.SaveChangesAsync();

                // Initialize RaceCount to 0 for new racer
                racer.RaceCount = 0;

                // Refresh the cached team list
                raceTeam = await GetRacersInternal();

                // Set as active racer if none is selected
                if (Active == null || !raceTeam.Any(r => r.Id == Active.Id)) {
                    Active = racer;
                }

                // Notify subscribers that the racer list has changed
                OnRacerChanged?.Invoke();
                
                Logger.LogInformation($"Added racer {racer.Name} with ID {racer.Id}");
                return racer;
            } catch (DuplicateRacerException) {
                throw; // Re-throw duplicate exceptions
            } catch (Exception ex) {
                Logger.LogError(ex, "Error adding racer {Name}", racer.Name);
                throw;
            }
        }

        public async Task UpdateRacer(Racer racer) {
            Logger.LogInformation("UpdateRacer for ID: {ID}", racer.Id);
            
            try {
                using var context = _contextFactory.CreateDbContext();
                var existingRacer = await context.Racers.FindAsync(racer.Id);
                if (existingRacer != null) {
                    existingRacer.Name = racer.Name;
                    existingRacer.AvatarFileName = racer.AvatarFileName;
                    existingRacer.LastRaced = racer.LastRaced;
                    existingRacer.LastPlayedRace = racer.LastPlayedRace;
                    
                    await context.SaveChangesAsync();
                    
                    // Refresh the cached team list
                    raceTeam = await GetRacersInternal();
                    
                    // Notify subscribers that the racer has been updated
                    OnRacerChanged?.Invoke();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error updating racer {ID}", racer.Id);
                throw;
            }
        }

        public async Task RemoveRacer(int racerId) {
            Logger.LogInformation("RemoveRacer for ID: {ID}", racerId);
            
            try {
                using var context = _contextFactory.CreateDbContext();
                var racer = await context.Racers.FindAsync(racerId);
                if (racer != null) {
                    // Remove all races associated with this racer first
                    var races = await context.Races.Where(r => r.RacerId == racerId).ToListAsync();
                    context.Races.RemoveRange(races);
                    
                    // Remove the racer
                    context.Racers.Remove(racer);
                    await context.SaveChangesAsync();
                    
                    // Refresh the cached team list
                    raceTeam = await GetRacersInternal();
                    
                    // If this was the active racer, set a new one
                    if (Active?.Id == racerId) {
                        Active = raceTeam.FirstOrDefault();
                    }
                    
                    // Notify subscribers that the racer has been removed
                    OnRacerChanged?.Invoke();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error removing racer {ID}", racerId);
                throw;
            }
        }

        public async Task<Racer> GetActiveRacer() {
            Logger.LogInformation("GetActiveRacer");
            
            // Ensure we have loaded the racers from database
            if (raceTeam.Count == 0) {
                raceTeam = await GetRacersInternal();
            }
            
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

            // If Active racer is not null, ensure it has the latest race count
            if (Active != null) {
                try {
                    // Update the race count using a separate query to avoid concurrency issues
                    using var context = _contextFactory.CreateDbContext();
                    var raceCount = await context.Races.CountAsync(r => r.RacerId == Active.Id);
                    Active.RaceCount = raceCount;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error updating race count for active racer");
                }
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
                    // If we can't get userId, use "guest" as the identifier
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
                // Use a separate scope for UserManager operations to avoid DbContext concurrency
                using (var scope = _serviceProvider.CreateScope()) {
                    var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    
                    // Get the current user
                    string userId = await GetUserID("GetLastPlayedRaceAsync");
                    var user = await scopedUserManager.FindByIdAsync(userId);
                    if (user != null && !string.IsNullOrEmpty(user.LastPlayedRace)) {
                        Logger.LogInformation($"Retrieved last played race '{user.LastPlayedRace}' for user {user.UserName}");
                        return user.LastPlayedRace;
                    }
                    return null;
                }
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
                var activeRacer = await GetActiveRacer();
                if (activeRacer != null) {
                    // Update the racer's last played race and last raced date
                    using var context = _contextFactory.CreateDbContext();
                    var racer = await context.Racers.FindAsync(activeRacer.Id);
                    if (racer != null) {
                        racer.LastPlayedRace = problemClassString;
                        racer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                        await context.SaveChangesAsync();
                        
                        // Update the active racer object as well
                        activeRacer.LastPlayedRace = problemClassString;
                        activeRacer.LastRaced = racer.LastRaced;
                        
                        Logger.LogInformation($"Saved last played race {problemClassString} for racer {racer.Name}");
                    }
                }

                // Use a separate scope for UserManager operations to avoid DbContext concurrency
                using (var scope = _serviceProvider.CreateScope()) {
                    var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    
                    // Also save to the user's profile
                    string userId = await GetUserID("SaveLastPlayedRace");
                    var user = await scopedUserManager.FindByIdAsync(userId);
                    if (user != null) {
                        user.LastPlayedRace = problemClassString;
                        await scopedUserManager.UpdateAsync(user);
                        Logger.LogInformation($"Saved last played race '{problemClassString}' for user {user.UserName}");
                        // Don't trigger OnRacerChanged for just saving last played race
                    }
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
                string userId = await GetUserID("GetTeamRaceCountAsync");
                if (!string.IsNullOrEmpty(userId)) {
                    // Use a single query with JOIN to avoid multiple database calls
                    using var context = _contextFactory.CreateDbContext();
                    int count = await context.Races
                        .Where(r => context.Racers.Any(racer => racer.UserId == userId && racer.Id == r.RacerId))
                        .CountAsync();
                    
                    Logger.LogInformation("Team race count for user {UserId}: {Count}", userId, count);
                    return count;
                }
                Logger.LogWarning("GetTeamRaceCountAsync: UserId is null or empty");
                return 0;
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
                // Get race count using a separate query to avoid concurrency issues
                using var context = _contextFactory.CreateDbContext();
                var raceCount = await context.Races.CountAsync(r => r.RacerId == racer.Id);
                racer.RaceCount = raceCount;
            }
            return racer;
        }

        /// <summary>
        /// Saves a completed race to the database for the current active racer
        /// </summary>
        /// <param name="totalTime">The total time taken to complete the race</param>
        /// <param name="problemClassString">The problem class string (e.g., "addition-4stable")</param>
        /// <param name="speedIncrements">The speed increments during the race</param>
        /// <returns>The saved race record</returns>
        public async Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null) {
            return await SaveRaceCompletionAsync(totalTime, GetProblemSetId(problemClassString), speedIncrements);
        }

        /// <summary>
        /// Saves a completed race to the database for the current active racer
        /// </summary>
        /// <param name="totalTime">The total time taken to complete the race</param>
        /// <param name="problemSetId">The identifier of the problem set (e.g., 1 for addition-4stable)</param>
        /// <param name="speedIncrements">The speed increments during the race</param>
        /// <returns>The saved race record</returns>
        public async Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null) {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer == null) {
                    throw new InvalidOperationException("No active racer found");
                }

                // Create the race record
                using var context = _contextFactory.CreateDbContext();
                var race = new Race {
                    RacerId = activeRacer.Id,
                    RaceDateTime = DateTime.Now,
                    TotalTime = totalTime,
                    ProblemSetId = problemSetId,
                    ImageId = activeRacer.Id % 6 + 1 // Cycle through available car images
                };

                // Add to database
                context.Races.Add(race);
                await context.SaveChangesAsync();

                // Add speed increments if provided
                if (speedIncrements != null && speedIncrements.Count > 0) {
                    foreach (var increment in speedIncrements) {
                        increment.RaceId = race.Id;
                    }
                    context.SpeedIncrements.AddRange(speedIncrements);
                    await context.SaveChangesAsync();
                }

                // Update the racer's last raced date
                var racer = await context.Racers.FindAsync(activeRacer.Id);
                if (racer != null) {
                    racer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                    await context.SaveChangesAsync();
                    
                    // Update the active racer object as well
                    activeRacer.LastRaced = racer.LastRaced;
                }

                Logger.LogInformation($"Saved race completion for racer {activeRacer.Name}: {totalTime:F2}s, ProblemSet {problemSetId}");
                
                // Notify that race counts may have changed
                OnRacerChanged?.Invoke();
                
                return race;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error saving race completion");
                throw;
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
