using DerbyDash.Data;
using DerbyDash.DTOs;
using DerbyDash.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace DerbyDash.Services
{
    public class ApiFAQService : IFAQService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiFAQService> _logger;
        private readonly FAQService _fallbackService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiFAQService(HttpClient httpClient, ILogger<ApiFAQService> logger, FAQService fallbackService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _fallbackService = fallbackService;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<FAQ>> GetAllFAQsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/problems/faq");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var faqDtos = JsonSerializer.Deserialize<List<FAQDto>>(json, _jsonOptions);
                    
                    return faqDtos?.Select(dto => new FAQ
                    {
                        Category = dto.Category,
                        Question = dto.Question,
                        Answer = dto.Answer
                    }).ToList() ?? new List<FAQ>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetAllFAQsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetAllFAQsAsync();
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/problems/faq/categories");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<string>>(json, _jsonOptions);
                    
                    return categories ?? new List<string>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetCategoriesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetCategoriesAsync();
            }
        }

        public async Task<List<FAQ>> GetFAQsByCategoryAsync(string category)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/problems/faq/category/{Uri.EscapeDataString(category)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var faqDtos = JsonSerializer.Deserialize<List<FAQDto>>(json, _jsonOptions);
                    
                    return faqDtos?.Select(dto => new FAQ
                    {
                        Category = dto.Category,
                        Question = dto.Question,
                        Answer = dto.Answer
                    }).ToList() ?? new List<FAQ>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetFAQsByCategoryAsync(category);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetFAQsByCategoryAsync(category);
            }
        }
    }
}
