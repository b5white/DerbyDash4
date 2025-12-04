using DerbyDash.Data;
using DerbyDash.Shared.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace DerbyDash.Services {
    public class ClientFAQService : IFAQService {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientFAQService(HttpClient httpClient) {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<FAQ>> GetAllFAQsAsync() {
            try {
                var response = await _httpClient.GetAsync("/api/problems/faq");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FAQDto>>>(_jsonOptions);
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        return apiResponse.Data.Select(dto => new FAQ {
                            Category = dto.Category,
                            Question = dto.Question,
                            Answer = dto.Answer
                        }).ToList();
                    }
                }
                return new List<FAQ>();
            } catch (Exception ex) {
                Console.WriteLine($"Error fetching FAQs: {ex.Message}");
                return new List<FAQ>();
            }
        }

        public async Task<List<string>> GetCategoriesAsync() {
            try {
                var response = await _httpClient.GetAsync("/api/problems/faq/categories");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<string>>>(_jsonOptions);
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        return apiResponse.Data;
                    }
                }
                return new List<string>();
            } catch (Exception ex) {
                Console.WriteLine($"Error fetching FAQ categories: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<List<FAQ>> GetFAQsByCategoryAsync(string category) {
            try {
                var encodedCategory = Uri.EscapeDataString(category);
                var response = await _httpClient.GetAsync($"/api/problems/faq/category/{encodedCategory}");
                if (response.IsSuccessStatusCode) {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<FAQDto>>>(_jsonOptions);
                    if (apiResponse?.Success == true && apiResponse.Data != null) {
                        return apiResponse.Data.Select(dto => new FAQ {
                            Category = dto.Category,
                            Question = dto.Question,
                            Answer = dto.Answer
                        }).ToList();
                    }
                }
                return new List<FAQ>();
            } catch (Exception ex) {
                Console.WriteLine($"Error fetching FAQs for category {category}: {ex.Message}");
                return new List<FAQ>();
            }
        }

        private class FAQDto {
            public int Id { get; set; }
            public string Category { get; set; } = string.Empty;
            public string Question { get; set; } = string.Empty;
            public string Answer { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public int Order { get; set; } = 0;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }
    }
}

