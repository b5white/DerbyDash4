using DerbyDash.Data;
using DerbyDash.DTOs;
using DerbyDash.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace DerbyDash.Services
{
    public class ApiProblemsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiProblemsService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiProblemsService(HttpClient httpClient, ILogger<ApiProblemsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Feedback>> GetFeedbackAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/problems/feedback");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var feedbackDtos = JsonSerializer.Deserialize<List<FeedbackDto>>(json, _jsonOptions);
                    
                    return feedbackDtos?.Select(dto => new Feedback
                    {
                        Id = dto.Id,
                        Title = dto.Title,
                        Description = dto.Description,
                        Status = dto.Status,
                        CreatedAt = dto.CreatedAt,
                        UpdatedAt = dto.UpdatedAt,
                        UserId = dto.UserId
                    }).ToList() ?? new List<Feedback>();
                }
                else
                {
                    _logger.LogWarning("Failed to get feedback from API: {StatusCode}", response.StatusCode);
                    return new List<Feedback>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feedback from API");
                return new List<Feedback>();
            }
        }

        public async Task<bool> CreateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var feedbackDto = new CreateFeedbackDto
                {
                    Title = feedback.Title,
                    Description = feedback.Description,
                    Status = feedback.Status
                };

                var json = JsonSerializer.Serialize(feedbackDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/problems/feedback", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to create feedback via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feedback via API");
                return false;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var feedbackDto = new UpdateFeedbackDto
                {
                    Title = feedback.Title,
                    Description = feedback.Description,
                    Status = feedback.Status
                };

                var json = JsonSerializer.Serialize(feedbackDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/problems/feedback/{feedback.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update feedback via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback via API");
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/problems/feedback/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete feedback via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feedback via API");
                return false;
            }
        }

        public async Task<List<FAQ>> GetFAQsAsync()
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
                        Id = dto.Id,
                        Question = dto.Question,
                        Answer = dto.Answer,
                        IsActive = dto.IsActive,
                        Order = dto.Order,
                        CreatedAt = dto.CreatedAt,
                        UpdatedAt = dto.UpdatedAt
                    }).ToList() ?? new List<FAQ>();
                }
                else
                {
                    _logger.LogWarning("Failed to get FAQs from API: {StatusCode}", response.StatusCode);
                    return new List<FAQ>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FAQs from API");
                return new List<FAQ>();
            }
        }

        public async Task<bool> CreateFAQAsync(FAQ faq)
        {
            try
            {
                var faqDto = new CreateFAQDto
                {
                    Question = faq.Question,
                    Answer = faq.Answer,
                    IsActive = faq.IsActive,
                    Order = faq.Order
                };

                var json = JsonSerializer.Serialize(faqDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/problems/faq", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to create FAQ via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating FAQ via API");
                return false;
            }
        }

        public async Task<bool> UpdateFAQAsync(FAQ faq)
        {
            try
            {
                var faqDto = new UpdateFAQDto
                {
                    Question = faq.Question,
                    Answer = faq.Answer,
                    IsActive = faq.IsActive,
                    Order = faq.Order
                };

                var json = JsonSerializer.Serialize(faqDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/problems/faq/{faq.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update FAQ via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ via API");
                return false;
            }
        }

        public async Task<bool> DeleteFAQAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/problems/faq/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to delete FAQ via API: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FAQ via API");
                return false;
            }
        }
    }
}
