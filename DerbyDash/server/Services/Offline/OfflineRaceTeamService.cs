using DerbyDash.Data;
using DerbyDash.Exceptions;

namespace DerbyDash.Services.Offline {
    public class OfflineRaceTeamService : IOfflineRaceTeamService {
        private readonly ILogger<OfflineRaceTeamService> Logger;
        private readonly List<Race> _offlineRaces = new();
        private int _nextRaceId = 1;

        // Offline race team - practice racers for offline mode
        private readonly List<Racer> raceTeam = new() {
            new Racer { Id = 1, Name = "Practice Racer 1", RaceCount = 0, LastRaced = null, LastPlayedRace = "addition-4stable", UserId = "offline" },
            new Racer { Id = 2, Name = "Practice Racer 2", RaceCount = 0, LastRaced = null, LastPlayedRace = "subtraction-4stable", UserId = "offline" },
            new Racer { Id = 3, Name = "Practice Racer 3", RaceCount = 0, LastRaced = null, LastPlayedRace = "multiplication-4stable", UserId = "offline" }
        };
        private Racer? Active;

        // Event that components can subscribe to for updates
        public event Func<Task>? OnRacerChanged;

        public OfflineRaceTeamService(ILogger<OfflineRaceTeamService> logger) {
            Logger = logger;
            // Set first racer as active by default for offline mode
            Active = raceTeam.FirstOrDefault();
        }

        public async Task<List<Racer>> GetRacers(bool includeCount = true) {
            Logger.LogInformation("GetRacers (Offline Mode)");
            await Task.CompletedTask;
            return raceTeam.ToList();
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount) {
            Logger.LogInformation("GetRacersByUserId (Offline Mode) for ID: {ID}", userId);
            await Task.CompletedTask;
            // In offline mode, all racers belong to the "offline" user
            return raceTeam.ToList();
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            Logger.LogInformation("GetRacerByIdAsync (Offline Mode) for ID: {ID}", racerId);
            Racer? racer = raceTeam.FirstOrDefault(r => r.Id == racerId);
            if (racer == null) {
                Logger.LogWarning("No Racer found for ID: {ID}", racerId);
            }
            await Task.CompletedTask;
            return racer;
        }

        public async Task<Racer> AddRacer(Racer racer) {
            Logger.LogInformation("AddRacer (Offline Mode) for ID: {ID}", racer.Id);
            racer.UserId = "offline";

            // Generate a unique ID if not provided
            if (racer.Id <= 0) {
                int maxId = raceTeam.Count > 0 ? raceTeam.Max(r => r.Id) : 0;
                racer.Id = maxId + 1;
            }
            racer.RaceCount = 0;
            raceTeam.Add(racer);

            // Set as active racer if this is the first racer
            if (raceTeam.Count == 1) {
                await SetActiveRacer(racer);
                Logger.LogInformation($"Set {racer.Name} as active racer (first racer for offline mode)");
            }

            // Notify subscribers that the racer list has changed
            await InvokeOnRacerChanged();
            return racer;
        }

        public async Task UpdateRacer(Racer racer) {
            Logger.LogInformation("UpdateRacer (Offline Mode) for ID: {ID}", racer.Id);
            var existingRacer = raceTeam.FirstOrDefault(r => r.Id == racer.Id);
            if (existingRacer != null) {
                existingRacer.Name = racer.Name;
                existingRacer.LastRaced = racer.LastRaced;
                existingRacer.LastPlayedRace = racer.LastPlayedRace;
                existingRacer.AvatarFileName = racer.AvatarFileName;
                existingRacer.RaceCount = racer.RaceCount;
            }
            await InvokeOnRacerChanged();
        }

        public async Task RemoveRacer(int racerId) {
            Logger.LogInformation("RemoveRacer (Offline Mode) for ID: {ID}", racerId);
            
            var racer = raceTeam.FirstOrDefault(r => r.Id == racerId);
            if (racer != null) {
                // Remove all races associated with this racer
                var racesToDelete = _offlineRaces.Where(r => r.RacerId == racerId).ToList();
                foreach (var race in racesToDelete) {
                    _offlineRaces.Remove(race);
                }
                
                // Remove the racer
                raceTeam.Remove(racer);
                
                // If this was the active racer, clear the active racer
                if (Active?.Id == racerId) {
                    Active = raceTeam.FirstOrDefault();
                }
                
                // Notify subscribers that the racer has been removed
                await InvokeOnRacerChanged();
            }
        }

        public async Task<Racer?> GetActiveRacer() {
            Logger.LogInformation("GetActiveRacer (Offline Mode)");
            await Task.CompletedTask;
            return Active;
        }

        public async Task SetActiveRacer(Racer racer) {
            Logger.LogInformation("SetActiveRacer (Offline Mode) ID: {ID}", racer.Id);
            Active = racer;
            
            // Notify subscribers that the active racer has changed
            await InvokeOnRacerChanged();
        }

        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer != null && !string.IsNullOrEmpty(activeRacer.LastPlayedRace)) {
                    Logger.LogInformation($"Retrieved last played race '{activeRacer.LastPlayedRace}' for racer {activeRacer.Name} (Offline Mode)");
                    return activeRacer.LastPlayedRace;
                }
                return null;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error retrieving last played race for active racer (Offline Mode)");
                return null;
            }
        }

        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            try {
                var activeRacer = await GetActiveRacer();
                if (activeRacer != null) {
                    activeRacer.LastPlayedRace = problemClassString;
                    activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Today);
                    
                    Logger.LogInformation($"Saved last played race '{problemClassString}' for racer {activeRacer.Name} (Offline Mode)");
                    
                    // Notify subscribers that the racer has been updated
                    await InvokeOnRacerChanged();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error saving last played race {ProblemClass} (Offline Mode)", problemClassString);
                throw;
            }
        }

        public async Task<int> GetTeamRaceCountAsync() {
            try {
                int count = _offlineRaces.Count;
                Logger.LogInformation("Team race count (Offline Mode): {Count}", count);
                await Task.CompletedTask;
                return count;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting team race count (Offline Mode)");
                return 0;
            }
        }

        public async Task<int> GetCurrentRacerRaceCountAsync() {
            try {
                var activeRacer = await GetRacerWithRaceCountAsync();
                return activeRacer?.RaceCount ?? 0;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting current racer race count (Offline Mode)");
                return 0;
            }
        }

        public async Task<Racer?> GetRacerWithRaceCountAsync() {
            var racer = await GetActiveRacer();
            if (racer != null) {
                // Calculate race count from offline races
                var raceCount = _offlineRaces.Count(r => r.RacerId == racer.Id);
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
                _offlineRaces.Add(race);

                // Update the racer's last raced date and race count
                activeRacer.LastRaced = DateOnly.FromDateTime(DateTime.Now);
                activeRacer.RaceCount = _offlineRaces.Count(r => r.RacerId == activeRacer.Id);

                Logger.LogInformation($"Saved race completion for racer {activeRacer.Name}: {totalTime:F2}s, ProblemSet {problemSetId}, Position {finishingPosition} (Offline Mode)");
                
                // Notify that race counts may have changed
                await InvokeOnRacerChanged();
                
                return race;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error saving race completion (Offline Mode)");
                throw;
            }
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId) {
            await Task.CompletedTask;
            // In offline mode, return all racers regardless of userId
            return raceTeam.ToList();
        }

        public async Task<int> GetTotalRacerCountAsync() {
            try {
                int count = raceTeam.Count;
                Logger.LogDebug("Retrieved total racer count (Offline Mode): {Count}", count);
                await Task.CompletedTask;
                return count;
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting total racer count (Offline Mode)");
                return 0;
            }
        }

        public async Task<Racer?> EnsureActiveRacerInitializedAsync() {
            var activeRacer = await GetActiveRacer();
            if (activeRacer == null && raceTeam.Count > 0) {
                await SetActiveRacer(raceTeam[0]);
                return raceTeam[0];
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
                _ => 999
            };
        }
    }
}
