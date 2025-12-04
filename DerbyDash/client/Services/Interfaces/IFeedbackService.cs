using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IFeedbackService {
        Task<bool> AddFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackByIdAsync(int id);
        Task<List<Feedback>> GetFeedbackByUserIdAsync(string userId);
        Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate);
        Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate);
        Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate);
        Task<bool> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> ResolveFeedbackAsync(int id, string adminNotes, string? publicResponse = null, string? privateResponse = null);
        Task<bool> DeleteFeedbackAsync(int id);
        Task SubmitFeedbackAsync(string subject, string message, bool canReply = false, string? contactEmail = null);
    }
}
