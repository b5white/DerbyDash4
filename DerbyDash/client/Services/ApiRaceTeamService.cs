using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using System.Net.Http.Json;

namespace DerbyDash.Services {
    public class ApiRaceTeamService {
        private readonly HttpClient _httpClient;

        public ApiRaceTeamService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<List<RacerDto>> GetRacersAsync() {
            var response = await _httpClient.GetAsync("/api/racers");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<RacerDto>>>();
                return result?.Data ?? new List<RacerDto>();
            }
            return new List<RacerDto>();
        }

        public async Task<RacerDto?> GetRacerByIdAsync(int id) {
            var response = await _httpClient.GetAsync($"/api/racers/{id}");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<RacerDto?> GetActiveRacerAsync() {
            var response = await _httpClient.GetAsync("/api/racers/active");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto?>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<RacerDto?> CreateRacerAsync(string name, string? avatarFileName = null) {
            var request = new CreateRacerRequest {
                Name = name,
                AvatarFileName = avatarFileName
            };
            var response = await _httpClient.PostAsJsonAsync("/api/racers", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<RacerDto?> UpdateRacerAsync(int id, string name, string? avatarFileName = null) {
            var request = new UpdateRacerRequest {
                Id = id,
                Name = name,
                AvatarFileName = avatarFileName
            };
            var response = await _httpClient.PutAsJsonAsync($"/api/racers/{id}", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RacerDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<bool> SetActiveRacerAsync(int id) {
            var response = await _httpClient.PostAsync($"/api/racers/{id}/set-active", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRacerAsync(int id) {
            var response = await _httpClient.DeleteAsync($"/api/racers/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> GetLastPlayedRaceAsync() {
            var response = await _httpClient.GetAsync("/api/racers/stats");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                // Parse the result to get LastPlayedRace
                if (result?.Data != null) {
                    var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
                    var doc = System.Text.Json.JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("lastPlayedRace", out var lastPlayedRace)) {
                        return lastPlayedRace.GetString();
                    }
                }
            }
            return null;
        }
    }
}

