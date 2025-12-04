using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Utilities;

namespace DerbyDash.Services {
    /// <summary>
    /// Client-side implementation of IOfflineRaceTeamService for offline mode
    /// Uses local storage to persist racers and races
    /// </summary>
    public class StubOfflineRaceTeamService : IOfflineRaceTeamService {
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<StubOfflineRaceTeamService>? _logger;
        private List<Racer> _racers = new();
        private List<Race> _races = new();
        private Racer? _activeRacer;
        private int _nextRaceId = 1;

        public event Func<Task>? OnRacerChanged;

        public StubOfflineRaceTeamService(ILocalStorageService localStorage, ILogger<StubOfflineRaceTeamService>? logger = null) {
            _localStorage = localStorage;
            _logger = logger;
            _ = InitializeAsync(); // Fire and forget initialization
        }

        private async Task InitializeAsync() {
            try {
                // Load racers from local storage
                var racers = await _localStorage.GetItemAsync<List<Racer>>("offline_racers");
                if (racers != null && racers.Any()) {
                    _racers = racers;
                } else {
                    // Initialize with default practice racers
                    _racers = new List<Racer> {
                        new Racer { Id = 1, Name = "Practice Racer 1", RaceCount = 0, LastRaced = null, LastPlayedRace = "addition-4stable", UserId = "offline", AvatarFileName = "1.jpg" },
                        new Racer { Id = 2, Name = "Practice Racer 2", RaceCount = 0, LastRaced = null, LastPlayedRace = "subtraction-4stable", UserId = "offline", AvatarFileName = "2.jpg" },
                        new Racer { Id = 3, Name = "Practice Racer 3", RaceCount = 0, LastRaced = null, LastPlayedRace = "multiplication-4stable", UserId = "offline", AvatarFileName = "3.jpg" }
                    };
                    await _localStorage.SetItemAsync("offline_racers", _racers);
                }

                // Load races from local storage
                var races = await _localStorage.GetItemAsync<List<Race>>("offline_races");
                if (races != null) {
                    _races = races;
                    if (_races.Any()) {
                        _nextRaceId = _races.Max(r => r.Id) + 1;
                    }
                }

                // Set active racer
                var activeRacerId = await _localStorage.GetItemAsync<int?>("offline_active_racer_id");
                if (activeRacerId.HasValue) {
                    _activeRacer = _racers.FirstOrDefault(r => r.Id == activeRacerId.Value);
                }
                if (_activeRacer == null && _racers.Any()) {
                    _activeRacer = _racers.First();
                    await _localStorage.SetItemAsync("offline_active_racer_id", _activeRacer.Id);
                }
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error initializing offline race team service");
            }
        }

        public async Task<List<Racer>> GetRacers(bool includeCount = true) {
            _logger?.LogInformation("GetRacers (Offline Mode)");
            if (!_racers.Any()) {
                await InitializeAsync();
            }
            return _racers.ToList();
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount) {
            _logger?.LogInformation("GetRacersByUserId (Offline Mode) for ID: {ID}", userId);
            var racers = await GetRacers(includeCount);
            // In offline mode, all racers belong to the "offline" user
            return racers.Where(r => r.UserId == userId || userId == "offline").ToList();
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId) {
            return await GetRacersByUserId(userId, true);
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            _logger?.LogInformation("GetRacerByIdAsync (Offline Mode) for ID: {ID}", racerId);
            var racers = await GetRacers();
            return racers.FirstOrDefault(r => r.Id == racerId);
        }

        public async Task<Racer> AddRacer(Racer racer) {
            _logger?.LogInformation("AddRacer (Offline Mode) for ID: {ID}", racer.Id);
            racer.UserId = "offline";

            // Generate a unique ID if not provided
            if (racer.Id <= 0) {
                var racers = await GetRacers();
                int maxId = racers.Count > 0 ? racers.Max(r => r.Id) : 0;
                racer.Id = maxId + 1;
            }
            racer.RaceCount = 0;
            _racers.Add(racer);
            await _localStorage.SetItemAsync("offline_racers", _racers);

            // Set as active racer if this is the first racer
            if (_racers.Count == 1) {
                await SetActiveRacer(racer);
                _logger?.LogInformation($"Set {racer.Name} as active racer (first racer for offline mode)");
            }

            await InvokeOnRacerChanged();
            return racer;
        }

        public async Task UpdateRacer(Racer racer) {
            _logger?.LogInformation("UpdateRacer (Offline Mode) for ID: {ID}", racer.Id);
            var existingRacer = _racers.FirstOrDefault(r => r.Id == racer.Id);
            if (existingRacer != null) {
                existingRacer.Name = racer.Name;
                existingRacer.LastRaced = racer.LastRaced;
                existingRacer.LastPlayedRace = racer.LastPlayedRace;
                existingRacer.AvatarFileName = racer.AvatarFileName;
                existingRacer.RaceCount = racer.RaceCount;
                await _localStorage.SetItemAsync("offline_racers", _racers);
                await InvokeOnRacerChanged();
            }
        }

        public async Task RemoveRacer(int racerId) {
            _logger?.LogInformation("RemoveRacer (Offline Mode) for ID: {ID}", racerId);
            
            var racer = _racers.FirstOrDefault(r => r.Id == racerId);
            if (racer != null) {
                // Remove all races associated with this racer
                var racesToDelete = _races.Where(r => r.RacerId == racerId).ToList();
                foreach (var race in racesToDelete) {
                    _races.Remove(race);
                }
                await _localStorage.SetItemAsync("offline_races", _races);
                
                // Remove the racer
                _racers.Remove(racer);
                await _localStorage.SetItemAsync("offline_racers", _racers);
                
                // If this was the active racer, clear the active racer
                if (_activeRacer?.Id == racerId) {
                    _activeRacer = _racers.FirstOrDefault();
                    if (_activeRacer != null) {
                        await _localStorage.SetItemAsync("offline_active_racer_id", _activeRacer.Id);
                    }
                }
                
                await InvokeOnRacerChanged();
            }
        }

        public async Task<Racer?> GetActiveRacer() {
            _logger?.LogInformation("GetActiveRacer (Offline Mode)");
            if (!_racers.Any()) {
                await InitializeAsync();
            }
            return _activeRacer;
        }

        public async Task SetActiveRacer(Racer racer) {
            _logger?.LogInformation("SetActiveRacer (Offline Mode) ID: {ID}", racer.Id);
            _activeRacer = racer;
            await _localStorage.SetItemAsync("offline_active_racer_id", racer.Id);
            await InvokeOnRacerChanged();
        }

        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer != null && !string.IsNullOrEmpty(activeRacer.LastPlayedRace)) {
                    _logger?.LogInformation($"Retrieved last played race '{activeRacer.LastPlayedRace}' for racer {activeRacer.Name} (Offline Mode)");
                    return activeRacer.LastPlayedRace;
                }
                return null;
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error retrieving last played race for active racer (Offline Mode)");
                return null;
            }
        }

        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer != null) {
                    activeRacer.LastPlayedRace = problemClassString;
                    activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Today);
                    await UpdateRacer(activeRacer);
                    _logger?.LogInformation($"Saved last played race '{problemClassString}' for racer {activeRacer.Name} (Offline Mode)");
                }
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error saving last played race {ProblemClass} (Offline Mode)", problemClassString);
                throw;
            }
        }

        public async Task<int> GetTeamRaceCountAsync() {
            try {
                int count = _races.Count;
                _logger?.LogInformation("Team race count (Offline Mode): {Count}", count);
                await Task.CompletedTask;
                return count;
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error getting team race count (Offline Mode)");
                return 0;
            }
        }

        public async Task<int> GetCurrentRacerRaceCountAsync() {
            try {
                var activeRacer = await GetRacerWithRaceCountAsync();
                return activeRacer?.RaceCount ?? 0;
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error getting current racer race count (Offline Mode)");
                return 0;
            }
        }

        public async Task<Racer?> GetRacerWithRaceCountAsync() {
            var racer = await GetActiveRacer();
            if (racer != null) {
                // Calculate race count from offline races
                var raceCount = _races.Count(r => r.RacerId == racer.Id);
                racer.RaceCount = raceCount;
            }
            return racer;
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            return await SaveRaceCompletionAsync(totalTime, GetProblemSetId(problemClassString), speedIncrements, finishingPosition);
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer == null) {
                    throw new InvalidOperationException("No active racer found");
                }

                // Create the race record
                var race = new Race {
                    Id = _nextRaceId++,
                    RacerId = activeRacer.Id,
                    RaceDateTime = DateTime.Now,
                    TotalTime = totalTime,
                    ProblemSetId = problemSetId,
                    ImageId = activeRacer.Id % 6 + 1,
                    FinishingPosition = finishingPosition,
                    SpeedIncrements = speedIncrements ?? new List<SpeedIncrement>()
                };

                // Add to offline storage
                _races.Add(race);
                await _localStorage.SetItemAsync("offline_races", _races);

                // Update the racer's last raced date and race count
                activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                activeRacer.RaceCount = _races.Count(r => r.RacerId == activeRacer.Id);
                await UpdateRacer(activeRacer);

                _logger?.LogInformation($"Saved race completion for racer {activeRacer.Name}: {totalTime:F2}s, ProblemSet {problemSetId}, Position {finishingPosition} (Offline Mode)");
                
                return race;
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error saving race completion (Offline Mode)");
                throw;
            }
        }

        public async Task<int> GetTotalRacerCountAsync() {
            try {
                var racers = await GetRacers();
                int count = racers.Count;
                _logger?.LogDebug("Retrieved total racer count (Offline Mode): {Count}", count);
                return count;
            } catch (Exception ex) {
                _logger?.LogError(ex, "Error getting total racer count (Offline Mode)");
                return 0;
            }
        }

        public async Task<Racer?> EnsureActiveRacerInitializedAsync() {
            var activeRacer = await GetActiveRacer();
            if (activeRacer == null && _racers.Count > 0) {
                await SetActiveRacer(_racers[0]);
                return _racers[0];
            }
            return activeRacer;
        }

        private async Task InvokeOnRacerChanged() {
            if (OnRacerChanged != null) {
                var handlers = OnRacerChanged.GetInvocationList().Cast<Func<Task>>();
                foreach (var handler in handlers) {
                    await handler();
                }
            }
        }

        private int GetProblemSetId(string problemClassString) {
            return UtilityMethods.GetUniqueIntFromString(problemClassString);
        }
    }
}
