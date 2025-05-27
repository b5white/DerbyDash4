using DerbyDash.Data;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Services {
    public class FeedbackService(ApplicationDbContext context) {

        public async Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate) {
            return await context.Feedbacks
                .Where(f => f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate) {
            return await context.Feedbacks
                .Where(f => f.FeedbackType == type && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate) {
            return await context.Feedbacks
                .Where(f => f.IsResolved == isResolved && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id) {
            return await context.Feedbacks.FindAsync(id);
        }

        public async Task<bool> AddFeedbackAsync(Feedback feedback) {
            try {
                feedback.SubmittedAt = DateTime.UtcNow;
                feedback.IsResolved = false;
                // TODO Populate racerId and UserId
                feedback.RacerId = 0;
                feedback.UserId = string.Empty;
                await context.Feedbacks.AddAsync(feedback);
                await context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
            	// TODO Use Logger instead of console
                Console.WriteLine($"Error adding feedback: {ex.Message}");
                if (ex.InnerException != null) {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback) {
            try {
                context.Feedbacks.Update(feedback);
                await context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes) {
            try {
                var feedback = await context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
                feedback.ResolvedAt = DateTime.UtcNow;

                context.Feedbacks.Update(feedback);
                await context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id) {
            try {
                var feedback = await context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return false;

                context.Feedbacks.Remove(feedback);
                await context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }
    }
}