using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Utilities.Logging;
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
        private readonly CurrentRequestDTO CurrentRequest;
        private readonly IJSRuntime _jsRuntime;
        private readonly IServiceProvider _serviceProvider;
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
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            CurrentRequestDTO currentRequest,
            IJSRuntime jsRuntime,
            IServiceProvider serviceProvider) {
            _userService = userService;
            Logger = logger;
            _contextFactory = contextFactory;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            CurrentRequest = currentRequest;
            _jsRuntime = jsRuntime;
            _serviceProvider = serviceProvider;        // Don't default the active racer during initialization
            // Active racer should only be set when first adding to race team or reading from cookie
            Active = null;
        }

        public async Task<List<Racer>> GetRacers() {
            Logger.LogInformation("GetRacers");
            raceTeam = await GetRacersInternal();
            return raceTeam;
        }

        public async Task<List<Racer>> GetRacersInternal() {
            Logger.LogInformation("GetRacersInternal");
            string userId;
            try {
                // Check if user is authenticated first
                if (!await _userService.IsLoggedInAsync()) {
                    // TODO throw an exception so we can redirect
                    Logger.LogWarning("User is not authenticated when trying to GetRacers");
                    return new List<Racer>();
                }

                // Try to get the userId, but don't fail if we can't
                try {
                    userId = await GetUserID("GetRacers");
                    Logger.LogInformation($"Getting racers for user: {userId}");
                } catch (Exception ex) {
                    Logger.LogWarning(ex, "Could not get userId, but continuing");
                    // TODO throw an exception so we can redirect them to log in
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
            return raceTeam;
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
            Logger.LogInformation("AddRacer for ID: {ID}", racer.Id);
            // Ensure user is authenticated
            if (!await _userService.IsLoggedInAsync()) {
                Logger.LogError("Attempt to add racer when user is not authenticated");
                throw new MissingUserException("User must be logged in to add a racer.");
            }
            racer.UserId = await GetUserID("AddRacer");
            Logger.LogInformation($"Using UserId: {racer.UserId} for new racer");

            // Ensure the userId exists in AspNetUsers
            using var context = _contextFactory.CreateDbContext();
            var userExists = await context.Users.AnyAsync(u => u.Id == racer.UserId);
            if (!userExists) {
                Logger.LogError($"UserId {racer.UserId} does not exist in AspNetUsers. Cannot add racer.");
                throw new MissingUserException($"UserId {racer.UserId} does not exist in AspNetUsers. Cannot add racer.");
            }

                // Check for duplicate names for this user
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

            // Set as active racer only if this is the first racer for the user (when race team is being created)
            if (Active is null && raceTeam.Count == 1) {
                Active = racer;
                Logger.LogInformation($"Set {racer.Name} as active racer (first racer for user)");
            }

            // Notify subscribers that the racer list has changed
            OnRacerChanged?.Invoke();
            await Task.CompletedTask; // Just to use 'await'
            return racer;
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
                    // Get all races associated with this racer
                    var races = await context.Races.Where(r => r.RacerId == racerId).ToListAsync();
                    
                    // For each race, delete its speed increments first
                    foreach (var race in races) {
                        var speedIncrements = await context.SpeedIncrements.Where(si => si.RaceId == race.Id).ToListAsync();
                        context.SpeedIncrements.RemoveRange(speedIncrements);
                    }
                    
                    // Then remove all races
                    context.Races.RemoveRange(races);
                    
                    // Finally remove the racer
                    context.Racers.Remove(racer);
                    
                    // Save all changes in a single transaction
                    await context.SaveChangesAsync();
                    
                    // Refresh the cached team list
                    raceTeam = await GetRacersInternal();
                    
                    // If this was the active racer, clear the active racer (don't auto-select new one)
                    if (Active?.Id == racerId) {
                        Active = null;
                    }
                    
                    // Notify subscribers that the racer has been removed
                    OnRacerChanged?.Invoke();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error removing racer {ID}", racerId);
                throw;
            }
        }

        /// <summary>
        /// Gets the currently active racer (cached) without any database queries.
        /// Call EnsureActiveRacerInitializedAsync() first to ensure it's loaded.
        /// </summary>
        public Racer? ActiveRacer => Active;

        /// <summary>
        /// Gets the active racer, initializing from cookie/database if needed (legacy method)
        /// Use ActiveRacer property for better performance when you know it's already initialized
        /// </summary>
        public async Task<Racer?> GetActiveRacer() {
            await EnsureActiveRacerInitializedAsync();
            return Active;
        }

        /// <summary>
        /// Ensures the active racer is initialized from cookie/database if needed.
        /// This should be called once at application startup or when needed.
        /// </summary>
        public async Task<Racer?> EnsureActiveRacerInitializedAsync() {
            Logger.LogInformation("EnsureActiveRacerInitializedAsync");
            
            // Try to load from cookie if we haven't already attempted to do so
            if (!_triedLoadingFromCookie) {
                try {
                    await LoadActiveRacerFromCookieAsync();
                    _triedLoadingFromCookie = true;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error loading active racer, using default");
                    // Don't default the active racer here - let it remain null
                }
            }

            // If no active racer but racers exist, set the first as active
            if (Active == null) {
                var racers = await GetRacersInternal();
                if (racers.Count > 0) {
                    Active = racers[0];
                    Logger.LogInformation($"No active racer was set, defaulting to first racer: {Active.Name} (ID: {Active.Id})");
                    // Optionally, persist this selection in the cookie
                    await SetActiveRacer(Active);
                }
            }

            return Active;
        }

        /// <summary>
        /// Updates the race count for the active racer from database.
        /// Call this only when you need the most up-to-date race count.
        /// </summary>
        public async Task UpdateActiveRacerRaceCountAsync() {
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
        }

        public async Task SetActiveRacer(Racer racer) {
            Logger.LogInformation("SetActiveRacer ID: {ID}", racer.Id);
            Active = racer;
            CurrentRequest.RacerId = Active.Id;

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
                    }                } else {
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
                // Get the active racer - use cached version if available
                var activeRacer = ActiveRacer ?? await GetActiveRacer();

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
                // Get the active racer - use cached version if available
                var activeRacer = ActiveRacer ?? await GetActiveRacer();

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
            // Use cached active racer if available
            var racer = ActiveRacer ?? await GetActiveRacer();
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
        /// <param name="finishingPosition">The finishing position in the race (1 = first place, etc.)</param>
        /// <returns>The saved race record</returns>
        public async Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            return await SaveRaceCompletionAsync(totalTime, GetProblemSetId(problemClassString), speedIncrements, finishingPosition);
        }

        /// <summary>
        /// Saves a completed race to the database for the current active racer
        /// </summary>
        /// <param name="totalTime">The total time taken to complete the race</param>
        /// <param name="problemSetId">The identifier of the problem set (e.g., 1 for addition-4stable)</param>
        /// <param name="speedIncrements">The speed increments during the race</param>
        /// <param name="finishingPosition">The finishing position in the race (1 = first place, etc.)</param>
        /// <returns>The saved race record</returns>
        public async Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            try {
                // Use cached active racer if available
                var activeRacer = ActiveRacer ?? await GetActiveRacer();
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
                    ImageId = activeRacer.Id % 6 + 1, // Cycle through available car images
                    FinishingPosition = finishingPosition
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

                Logger.LogInformation($"Saved race completion for racer {activeRacer.Name}: {totalTime:F2}s, ProblemSet {problemSetId}, Position {finishingPosition}");
                
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

        public async Task<List<Racer>> GetRacersByUserId(string userId)
        {
            using var context = _contextFactory.CreateDbContext();
            var racers = await context.Racers
                .Where(r => r.UserId == userId)
                .Select(r => new Racer
                {
                    Id = r.Id,
                    Name = r.Name,
                    UserId = r.UserId,
                    LastRaced = r.LastRaced,
                    LastPlayedRace = r.LastPlayedRace,
                    AvatarFileName = r.AvatarFileName,
                    RaceCount = context.Races.Count(race => race.RacerId == r.Id)
                })
                .ToListAsync();

            return racers;
        }

        public async Task<int> GetTotalRacerCountAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var count = await context.Racers.CountAsync();
                Logger.LogDebug("Retrieved total racer count: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting total racer count");
                return 0;
            }
        }
    }
}
