using DerbyDash.Data;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Services {
    public class FeedbackService {
        private readonly ApplicationDbContext _context;

        public FeedbackService(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<List<Feedback>> GetAllFeedbackAsync() {
            return await _context.Feedbacks
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByTypeAsync(string type) {
            return await _context.Feedbacks
                .Where(f => f.FeedbackType == type)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved) {
            return await _context.Feedbacks
                .Where(f => f.IsResolved == isResolved)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id) {
            return await _context.Feedbacks.FindAsync(id);
        }

        public async Task<bool> AddFeedbackAsync(Feedback feedback) {
            try {
                feedback.SubmittedAt = DateTime.UtcNow;
                feedback.IsResolved = false;

                await _context.Feedbacks.AddAsync(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback) {
            try {
                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes) {
            try {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
                feedback.ResolvedAt = DateTime.UtcNow;

                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id) {
            try {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                    return false;

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch {
                return false;
            }
        }
    }
}