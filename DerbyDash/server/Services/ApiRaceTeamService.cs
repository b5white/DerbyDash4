using DerbyDash.Data;
using DerbyDash.Services;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DerbyDash.Services {
    public class ApiRaceTeamService : IRaceTeamService {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly ILogger<ApiRaceTeamService> _logger;
        
        // Fallback to original service for non-API operations
        private readonly RaceTeamService _fallbackService;

        // Event that components can subscribe to for updates
        public event Func<Task>? OnRacerChanged;

        public ApiRaceTeamService(
            HttpClient httpClient,
            IUserService userService,
            ILogger<ApiRaceTeamService> logger,
            RaceTeamService fallbackService) {
            _httpClient = httpClient;
            _userService = userService;
            _logger = logger;
            _fallbackService = fallbackService;
        }

        private bool TrySetAuthHeader() {
            try {
                // Add internal call header to bypass authentication for server-side calls
                _httpClient.DefaultRequestHeaders.Remove("X-Internal-Call");
                _httpClient.DefaultRequestHeaders.Add("X-Internal-Call", "true");
                return true;
            } catch {
                return false;
            }
        }

        public async Task<List<Racer>> GetRacers(bool includeCount = true) {
            try {
                _logger.LogInformation("ApiRaceTeamService.GetRacers - Attempting API call");
                
                if (!TrySetAuthHeader()) {
                    _logger.LogWarning("Could not set auth header, falling back to original service");
                    return await _fallbackService.GetRacers(includeCount);
                }

                var response = await _httpClient.GetAsync("/api/racers");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<RacerDto>>>();
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        // Convert DTOs to domain objects
                        var racers = apiResponse.Data.Select(dto => new Racer {
                            Id = dto.Id,
                            UserId = dto.UserId,
                            Name = dto.Name,
                            LastRaced = dto.LastRaced,
                            LastPlayedRace = dto.LastPlayedRace,
                            AvatarFileName = dto.AvatarFileName,
                            RaceCount = dto.RaceCount
                        }).ToList();
                        
                        _logger.LogInformation($"Successfully loaded {racers.Count} racers from API");
                        return racers;
                    }
                }
                
                _logger.LogWarning($"API call failed with status {response.StatusCode}, falling back to original service");
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling racers API, falling back to original service");
            }
            
            // Fallback to original service
            return await _fallbackService.GetRacers(includeCount);
        }

        public async Task<List<Racer>> GetRacersByUserId(string userId, bool includeCount) {
            // For now, fallback to original service for user-specific queries
            return await _fallbackService.GetRacersByUserId(userId, includeCount);
        }

        public async Task<Racer?> GetRacerByIdAsync(int racerId) {
            try {
                if (!TrySetAuthHeader()) {
                    return await _fallbackService.GetRacerByIdAsync(racerId);
                }

                var response = await _httpClient.GetAsync($"/api/racers/{racerId}");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto>>();
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        var dto = apiResponse.Data;
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
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling racer API for ID {RacerId}", racerId);
            }
            
            return await _fallbackService.GetRacerByIdAsync(racerId);
        }

        public async Task<Racer> AddRacer(Racer racer) {
            try {
                if (!TrySetAuthHeader()) {
                    return await _fallbackService.AddRacer(racer);
                }

                var request = new CreateRacerRequest {
                    Name = racer.Name,
                    AvatarFileName = racer.AvatarFileName
                };

                var response = await _httpClient.PostAsJsonAsync("/api/racers", request);
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto>>();
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        var dto = apiResponse.Data;
                        var newRacer = new Racer {
                            Id = dto.Id,
                            UserId = dto.UserId,
                            Name = dto.Name,
                            LastRaced = dto.LastRaced,
                            LastPlayedRace = dto.LastPlayedRace,
                            AvatarFileName = dto.AvatarFileName,
                            RaceCount = dto.RaceCount
                        };
                        
                        // Notify listeners
                        if (OnRacerChanged != null) {
                            await OnRacerChanged.Invoke();
                        }
                        
                        return newRacer;
                    }
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling add racer API");
            }
            
            return await _fallbackService.AddRacer(racer);
        }

        public async Task UpdateRacer(Racer racer) {
            try {
                if (!TrySetAuthHeader()) {
                    await _fallbackService.UpdateRacer(racer);
                    return;
                }

                var request = new UpdateRacerRequest {
                    Id = racer.Id,
                    Name = racer.Name,
                    AvatarFileName = racer.AvatarFileName
                };

                var response = await _httpClient.PutAsJsonAsync($"/api/racers/{racer.Id}", request);
                if (response.IsSuccessStatusCode) {
                    // Notify listeners
                    if (OnRacerChanged != null) {
                        await OnRacerChanged.Invoke();
                    }
                    return;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling update racer API");
            }
            
            await _fallbackService.UpdateRacer(racer);
        }

        public async Task RemoveRacer(int racerId) {
            try {
                if (!TrySetAuthHeader()) {
                    await _fallbackService.RemoveRacer(racerId);
                    return;
                }

                var response = await _httpClient.DeleteAsync($"/api/racers/{racerId}");
                if (response.IsSuccessStatusCode) {
                    // Notify listeners
                    if (OnRacerChanged != null) {
                        await OnRacerChanged.Invoke();
                    }
                    return;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling delete racer API");
            }
            
            await _fallbackService.RemoveRacer(racerId);
        }

        public async Task<Racer?> GetActiveRacer() {
            try {
                if (!TrySetAuthHeader()) {
                    return await _fallbackService.GetActiveRacer();
                }

                var response = await _httpClient.GetAsync("/api/racers/active");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto?>>();
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        var dto = apiResponse.Data;
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
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling active racer API");
            }
            
            return await _fallbackService.GetActiveRacer();
        }

        public async Task SetActiveRacer(Racer racer) {
            try {
                if (!TrySetAuthHeader()) {
                    await _fallbackService.SetActiveRacer(racer);
                    return;
                }

                var response = await _httpClient.PostAsync($"/api/racers/{racer.Id}/set-active", null);
                if (response.IsSuccessStatusCode) {
                    return;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling set active racer API");
            }
            
            await _fallbackService.SetActiveRacer(racer);
        }

        // Delegate other methods to the fallback service for now
        public async Task<Racer?> GetRacerWithRaceCountAsync() {
            return await _fallbackService.GetRacerWithRaceCountAsync();
        }

        public async Task<string?> GetLastPlayedRaceAsync() {
            return await _fallbackService.GetLastPlayedRaceAsync();
        }

        public async Task SaveLastPlayedRaceAsync(string problemClassString) {
            await _fallbackService.SaveLastPlayedRaceAsync(problemClassString);
        }

        public async Task<int> GetTeamRaceCountAsync() {
            return await _fallbackService.GetTeamRaceCountAsync();
        }

        public async Task<int> GetCurrentRacerRaceCountAsync() {
            return await _fallbackService.GetCurrentRacerRaceCountAsync();
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, string problemClassString, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            return await _fallbackService.SaveRaceCompletionAsync(totalTime, problemClassString, speedIncrements, finishingPosition);
        }

        public async Task<Race> SaveRaceCompletionAsync(double totalTime, int problemSetId, List<SpeedIncrement>? speedIncrements = null, int finishingPosition = 0) {
            return await _fallbackService.SaveRaceCompletionAsync(totalTime, problemSetId, speedIncrements, finishingPosition);
        }

        public async Task<int> GetTotalRacerCountAsync() {
            return await _fallbackService.GetTotalRacerCountAsync();
        }

        public async Task<Racer?> EnsureActiveRacerInitializedAsync() {
            return await _fallbackService.EnsureActiveRacerInitializedAsync();
        }
    }
}
