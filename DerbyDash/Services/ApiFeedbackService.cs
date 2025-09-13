using DerbyDash.Data;
using DerbyDash.DTOs;
using DerbyDash.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace DerbyDash.Services
{
    public class ApiFeedbackService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiFeedbackService> _logger;
        private readonly FeedbackService _fallbackService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiFeedbackService(HttpClient httpClient, ILogger<ApiFeedbackService> logger, IUserService userService, IRaceTeamService raceTeamService, ILogger<FeedbackService> feedbackLogger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _fallbackService = new FeedbackService(userService, raceTeamService, feedbackLogger);
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/problems/feedback?since={startDate:yyyy-MM-ddTHH:mm:ss}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var feedbackDtos = JsonSerializer.Deserialize<List<FeedbackDto>>(json, _jsonOptions);
                    
                    return feedbackDtos?.Select(ConvertFromDto).Where(f => f.SubmittedAt >= startDate).ToList() ?? new List<Feedback>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetFeedbackSinceDateAsync(startDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetFeedbackSinceDateAsync(startDate);
            }
        }

        public async Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/problems/feedback?type={type}&since={startDate:yyyy-MM-ddTHH:mm:ss}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var feedbackDtos = JsonSerializer.Deserialize<List<FeedbackDto>>(json, _jsonOptions);
                    
                    return feedbackDtos?.Select(ConvertFromDto).Where(f => f.FeedbackType == type && f.SubmittedAt >= startDate).ToList() ?? new List<Feedback>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetFeedbackByTypeAsync(type, startDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetFeedbackByTypeAsync(type, startDate);
            }
        }

        public async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/problems/feedback?resolved={isResolved}&since={startDate:yyyy-MM-ddTHH:mm:ss}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var feedbackDtos = JsonSerializer.Deserialize<List<FeedbackDto>>(json, _jsonOptions);
                    
                    return feedbackDtos?.Select(ConvertFromDto).Where(f => f.IsResolved == isResolved && f.SubmittedAt >= startDate).ToList() ?? new List<Feedback>();
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetFeedbackByStatusAsync(isResolved, startDate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetFeedbackByStatusAsync(isResolved, startDate);
            }
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/problems/feedback/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var feedbackDto = JsonSerializer.Deserialize<FeedbackDto>(json, _jsonOptions);
                    
                    return feedbackDto != null ? ConvertFromDto(feedbackDto) : null;
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.GetFeedbackByIdAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.GetFeedbackByIdAsync(id);
            }
        }

        public async Task<bool> AddFeedbackAsync(Feedback feedback)
        {
            try
            {
                var feedbackDto = ConvertToCreateDto(feedback);
                var json = JsonSerializer.Serialize(feedbackDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/problems/feedback", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.AddFeedbackAsync(feedback);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.AddFeedbackAsync(feedback);
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback)
        {
            try
            {
                var feedbackDto = ConvertToUpdateDto(feedback);
                var json = JsonSerializer.Serialize(feedbackDto, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/problems/feedback/{feedback.Id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.UpdateFeedbackAsync(feedback);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.UpdateFeedbackAsync(feedback);
            }
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes)
        {
            try
            {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null) return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
                feedback.ResolvedAt = DateTime.UtcNow;

                return await UpdateFeedbackAsync(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.ResolveFeedbackAsync(id, adminNotes);
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
                    _logger.LogWarning("API call failed, falling back to original service");
                    return await _fallbackService.DeleteFeedbackAsync(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API, falling back to original service");
                return await _fallbackService.DeleteFeedbackAsync(id);
            }
        }

        public List<Feedback> GenerateFakeFeedbacks(int count = 3)
        {
            return _fallbackService.GenerateFakeFeedbacks(count);
        }

        private Feedback ConvertFromDto(FeedbackDto dto)
        {
            return new Feedback
            {
                Id = dto.Id,
                UserId = dto.UserId,
                RacerId = dto.RacerId,
                Name = dto.Name,
                Email = dto.Email,
                FeedbackType = dto.FeedbackType,
                Subject = string.IsNullOrEmpty(dto.Subject) ? dto.Title : dto.Subject,
                Message = string.IsNullOrEmpty(dto.Message) ? dto.Description : dto.Message,
                BrowserInfo = dto.BrowserInfo,
                ContactConsent = dto.ContactConsent,
                SubmittedAt = dto.SubmittedAt != default ? dto.SubmittedAt : dto.CreatedAt,
                IsResolved = dto.IsResolved,
                AdminNotes = dto.AdminNotes,
                PublicResponse = dto.PublicResponse,
                PrivateResponse = dto.PrivateResponse,
                ResolvedAt = dto.ResolvedAt
            };
        }

        private CreateFeedbackDto ConvertToCreateDto(Feedback feedback)
        {
            return new CreateFeedbackDto
            {
                Name = feedback.Name,
                Email = feedback.Email,
                FeedbackType = feedback.FeedbackType,
                Subject = feedback.Subject,
                Message = feedback.Message,
                BrowserInfo = feedback.BrowserInfo,
                ContactConsent = feedback.ContactConsent,
                Title = feedback.Subject,
                Description = feedback.Message,
                Status = feedback.IsResolved ? "Resolved" : "Open"
            };
        }

        private UpdateFeedbackDto ConvertToUpdateDto(Feedback feedback)
        {
            return new UpdateFeedbackDto
            {
                Name = feedback.Name,
                Email = feedback.Email,
                FeedbackType = feedback.FeedbackType,
                Subject = feedback.Subject,
                Message = feedback.Message,
                BrowserInfo = feedback.BrowserInfo,
                ContactConsent = feedback.ContactConsent,
                IsResolved = feedback.IsResolved,
                AdminNotes = feedback.AdminNotes,
                PublicResponse = feedback.PublicResponse,
                PrivateResponse = feedback.PrivateResponse,
                Title = feedback.Subject,
                Description = feedback.Message,
                Status = feedback.IsResolved ? "Resolved" : "Open"
            };
        }
    }
}
