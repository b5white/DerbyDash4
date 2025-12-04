using DerbyDash.Data;
using DerbyDash.Exceptions;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;

namespace DerbyDash.Services {
    /// <summary>
    /// Client-side adapter that implements IRaceTeamService by calling API services
    /// Converts between DTOs and Data models
    /// </summary>
    public class ClientRaceTeamService : IRaceTeamService {
        private readonly ApiRaceTeamService _apiService;
        private readonly ApiRaceService _apiRaceService;
        private readonly IUserService _userService;
        private readonly ILogger<ClientRaceTeamService> _logger;
        private Racer? _cachedActiveRacer;

        public event Func<Task>? OnRacerChanged;

        public ClientRaceTeamService(
            ApiRaceTeamService apiService,
            ApiRaceService apiRaceService,
            IUserService userService,
            ILogger<ClientRaceTeamService> logger) {
            _apiService = apiService;
            _apiRaceService = apiRaceService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<List<Racer>> GetRacers(bool includeCount = true) {
            try {
                var racerDtos = await _apiService.GetRacersAsync();
                return racerDtos.Select(dto => ConvertToRacer(dto)).ToList();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting racers");
                return new List<Racer>();
            }
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount) {
            // For client, we get all racers and filter by userId
            var allRacers = await GetRacers(includeCount);
            return allRacers.Where(r => r.UserId == userId).ToList();
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            try {
                var dto = await _apiService.GetRacerByIdAsync(racerId);
                return dto != null ? ConvertToRacer(dto) : null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting racer {RacerId}", racerId);
                return null;
            }
        }

        public async Task<Racer?> GetRacerWithRaceCountAsync() {
            return await GetActiveRacer();
        }

        public async Task<Racer> AddRacer(Racer racer) {
            try {
                var dto = await _apiService.CreateRacerAsync(racer.Name, racer.AvatarFileName);
                if (dto == null) {
                    throw new Exception("Failed to create racer");
                }
                var newRacer = ConvertToRacer(dto);
                await NotifyRacerChanged();
                return newRacer;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding racer");
                throw;
            }
        }

        public async Task UpdateRacer(Racer racer) {
            try {
                var dto = await _apiService.UpdateRacerAsync(racer.Id, racer.Name, racer.AvatarFileName);
                if (dto == null) {
                    throw new Exception("Failed to update racer");
                }
                await NotifyRacerChanged();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error updating racer");
                throw;
            }
        }

        public async Task RemoveRacer(int racerId) {
            try {
                var success = await _apiService.DeleteRacerAsync(racerId);
                if (!success) {
                    throw new Exception("Failed to delete racer");
                }
                if (_cachedActiveRacer?.Id == racerId) {
                    _cachedActiveRacer = null;
                }
                await NotifyRacerChanged();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error removing racer");
                throw;
            }
        }

        public async Task<Racer?> GetActiveRacer() {
            try {
                // Use cached value if available
                if (_cachedActiveRacer != null) {
                    return _cachedActiveRacer;
                }

                var dto = await _apiService.GetActiveRacerAsync();
                if (dto != null) {
                    _cachedActiveRacer = ConvertToRacer(dto);
                    return _cachedActiveRacer;
                }
                return null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting active racer");
                return null;
            }
        }

        public async Task SetActiveRacer(Racer racer) {
            try {
                var success = await _apiService.SetActiveRacerAsync(racer.Id);
                if (success) {
                    _cachedActiveRacer = racer;
                    await NotifyRacerChanged();
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error setting active racer");
                throw;
            }
        }

        public async Task<string?> GetLastPlayedRaceAsync() {
            try {
                return await _apiService.GetLastPlayedRaceAsync();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting last played race");
                return null;
            }
        }

        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            // This would need to be implemented in the API
            // For now, we'll save it when saving a race
            await Task.CompletedTask;
        }

        public async Task<int> GetTeamRaceCountAsync() {
            // This would need to be implemented in the API
            // For now, return 0
            await Task.CompletedTask;
            return 0;
        }

        public async Task<int> GetCurrentRacerRaceCountAsync() {
            var activeRacer = await GetActiveRacer();
            return activeRacer?.RaceCount ?? 0;
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            try {
                var speedIncrementData = speedIncrements?.Select(si => new SpeedIncrementData {
                    Time = si.Time,
                    Speed = si.Speed,
                    Distance = si.Distance
                }).ToList();

                // Call API race service to save the race
                var raceDto = await _apiRaceService.SaveRaceAsync(totalTime, problemClassString, speedIncrementData, finishingPosition);
                
                if (raceDto == null) {
                    throw new Exception("Failed to save race");
                }

                // Convert DTO to Race model
                var race = ConvertToRace(raceDto);
                
                // Update active racer's last played race
                await SaveLastPlayedRaceAsync(problemClassString);
                
                return race;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error saving race completion");
                throw;
            }
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            // This would need problemSetId to problemClassString mapping
            throw new NotImplementedException("Use SaveRaceCompletionAsync with problemClassString instead");
        }

        public async Task<int> GetTotalRacerCountAsync() {
            var racers = await GetRacers();
            return racers.Count;
        }

        public async Task<Racer?> EnsureActiveRacerInitializedAsync() {
            var activeRacer = await GetActiveRacer();
            if (activeRacer != null) {
                return activeRacer;
            }

            // If no active racer, get the first racer
            var racers = await GetRacers();
            if (racers.Count > 0) {
                await SetActiveRacer(racers[0]);
                return racers[0];
            }

            return null;
        }

        private Racer ConvertToRacer(RacerDto dto) {
            return new Racer {
                Id = dto.Id,
                UserId = dto.UserId,
                Name = dto.Name,
                LastRaced = dto.LastRaced,
                LastPlayedRace = dto.LastPlayedRace,
                AvatarFileName = dto.AvatarFileName,
                RaceCount = dto.RaceCount
            };
        }

        private Race ConvertToRace(RaceDto dto) {
            return new Race {
                Id = dto.Id,
                RacerId = dto.RacerId,
                RaceDateTime = dto.RaceDateTime,
                TotalTime = dto.TotalTime,
                ProblemSetId = dto.ProblemSetId,
                ImageId = dto.ImageId,
                FinishingPosition = dto.FinishingPosition,
                SpeedIncrements = dto.SpeedIncrements?.Select(si => new SpeedIncrement {
                    Id = si.Id,
                    RaceId = si.RaceId,
                    Time = si.Time,
                    Speed = si.Speed,
                    Distance = si.Distance
                }).ToList()
            };
        }

        private async Task NotifyRacerChanged() {
            if (OnRacerChanged != null) {
                await OnRacerChanged.Invoke();
            }
        }
    }
}
