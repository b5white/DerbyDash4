using DerbyDash.Data;
using DerbyDash.Services;
using Microsoft.Extensions.Logging;

namespace DerbyDash.Services
{
    public class FeedbackServiceWrapper : FeedbackService
    {
        private readonly ApiFeedbackService _apiService;

        public FeedbackServiceWrapper(ApiFeedbackService apiService, IUserService userService, IRaceTeamService raceTeamService, ILogger<FeedbackService> logger)
            : base(userService, raceTeamService, logger)
        {
            _apiService = apiService;
        }

        public override async Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate)
        {
            return await _apiService.GetFeedbackSinceDateAsync(startDate);
        }

        public override async Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate)
        {
            return await _apiService.GetFeedbackByTypeAsync(type, startDate);
        }

        public override async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate)
        {
            return await _apiService.GetFeedbackByStatusAsync(isResolved, startDate);
        }

        public override async Task<Feedback?> GetFeedbackByIdAsync(int id)
        {
            return await _apiService.GetFeedbackByIdAsync(id);
        }

        public override async Task<bool> AddFeedbackAsync(Feedback feedback)
        {
            return await _apiService.AddFeedbackAsync(feedback);
        }

        public override async Task<bool> UpdateFeedbackAsync(Feedback feedback)
        {
            return await _apiService.UpdateFeedbackAsync(feedback);
        }

        public override async Task<bool> ResolveFeedbackAsync(int id, string adminNotes)
        {
            return await _apiService.ResolveFeedbackAsync(id, adminNotes);
        }

        public override async Task<bool> DeleteFeedbackAsync(int id)
        {
            return await _apiService.DeleteFeedbackAsync(id);
        }

        public override List<Feedback> GenerateFakeFeedbacks(int count = 3)
        {
            return _apiService.GenerateFakeFeedbacks(count);
        }
    }
}
