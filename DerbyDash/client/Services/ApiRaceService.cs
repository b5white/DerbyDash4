using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using System.Net.Http.Json;

namespace DerbyDash.Services {
    public class ApiRaceService {
        private readonly HttpClient _httpClient;

        public ApiRaceService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<RaceDto?> SaveRaceAsync(double totalTime, string problemClassString, 
            List<SpeedIncrementData>? speedIncrements, int finishingPosition) {
            var request = new SaveRaceRequest {
                TotalTime = totalTime,
                ProblemClassString = problemClassString,
                SpeedIncrements = speedIncrements,
                FinishingPosition = finishingPosition
            };
            var response = await _httpClient.PostAsJsonAsync("/api/race/save", request);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RaceDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<PaginatedResponse<RaceDto>> GetRaceHistoryAsync(int page = 1, int pageSize = 10, int? racerId = null) {
            var url = $"/api/race/history?page={page}&pageSize={pageSize}";
            if (racerId.HasValue) {
                url += $"&racerId={racerId.Value}";
            }
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedResponse<RaceDto>>>();
                return result?.Data ?? new PaginatedResponse<RaceDto>();
            }
            return new PaginatedResponse<RaceDto>();
        }

        public async Task<RaceDto?> GetRaceAsync(int id) {
            var response = await _httpClient.GetAsync($"/api/race/{id}");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<RaceDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<bool> DeleteRaceAsync(int id) {
            var response = await _httpClient.DeleteAsync($"/api/race/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}

