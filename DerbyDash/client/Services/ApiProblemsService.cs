using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Problems;
using System.Net.Http.Json;

namespace DerbyDash.Services {
    public class ApiProblemsService {
        private readonly HttpClient _httpClient;

        public ApiProblemsService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<ProblemSetDto?> GetProblemsAsync(string problemType) {
            var response = await _httpClient.GetAsync($"/api/problems/{problemType}");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProblemSetDto>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<string?> GetProblemTitleAsync(string problemType) {
            var response = await _httpClient.GetAsync($"/api/problems/{problemType}/title");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                return result?.Data;
            }
            return null;
        }

        public async Task<List<string>> GetAvailableProblemTypesAsync() {
            var response = await _httpClient.GetAsync("/api/problems");
            if (response.IsSuccessStatusCode) {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>();
                return result?.Data ?? new List<string>();
            }
            return new List<string>();
        }
    }
}

