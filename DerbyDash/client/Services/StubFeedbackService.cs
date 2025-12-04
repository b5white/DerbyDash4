using DerbyDash.Data;

namespace DerbyDash.Services {
    public class StubFeedbackService : IFeedbackService {
        public Task<bool> AddFeedbackAsync(Feedback feedback) {
            // TODO: Implement API call to add feedback
            return Task.FromResult(true);
        }

        public Task<Feedback?> GetFeedbackByIdAsync(int id) {
            // TODO: Implement API call to get feedback by ID
            return Task.FromResult<Feedback?>(null);
        }

        public Task<List<Feedback>> GetFeedbackByUserIdAsync(string userId) {
            // TODO: Implement API call to get feedback by user ID
            return Task.FromResult(new List<Feedback>());
        }

        public Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate) {
            // TODO: Implement API call to get feedback since date
            return Task.FromResult(new List<Feedback>());
        }

        public Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate) {
            // TODO: Implement API call to get feedback by type
            return Task.FromResult(new List<Feedback>());
        }

        public Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate) {
            // TODO: Implement API call to get feedback by status
            return Task.FromResult(new List<Feedback>());
        }

        public Task<bool> UpdateFeedbackAsync(Feedback feedback) {
            // TODO: Implement API call to update feedback
            return Task.FromResult(false);
        }

        public Task<bool> ResolveFeedbackAsync(int id, string adminNotes, string? publicResponse = null, string? privateResponse = null) {
            // TODO: Implement API call to resolve feedback
            return Task.FromResult(false);
        }

        public Task<bool> DeleteFeedbackAsync(int id) {
            // TODO: Implement API call to delete feedback
            return Task.FromResult(false);
        }

        public Task SubmitFeedbackAsync(string subject, string message, bool canReply = false, string? contactEmail = null) {
            // TODO: Implement API call to submit feedback
            return Task.CompletedTask;
        }
    }
}
