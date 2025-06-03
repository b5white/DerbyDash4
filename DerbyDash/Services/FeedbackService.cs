using Bogus;
using DerbyDash.Data;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Services {
    public class FeedbackService {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IRaceTeamService _raceTeamService;
        private readonly ILogger<FeedbackService> _logger;

        public FeedbackService(ApplicationDbContext context, IUserService userService, IRaceTeamService raceTeamService, ILogger<FeedbackService> logger) {
            _context = context;
            _userService = userService;
            _raceTeamService = raceTeamService;
            _logger = logger;
        }

        public async Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate) {
            return await _context.Feedbacks
                .Where(f => f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate) {
            return await _context.Feedbacks
                .Where(f => f.FeedbackType == type && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate) {
            return await _context.Feedbacks
                .Where(f => f.IsResolved == isResolved && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id) {
            return await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<bool> AddFeedbackAsync(Feedback feedback) {
            try {
                feedback.SubmittedAt = DateTime.UtcNow;
                feedback.IsResolved = false;
                
                // Get current userId if user is authenticated
                try {
                    if (await _userService.IsLoggedInAsync()) {
                        feedback.UserId = await _userService.GetUserIdAsync("AddFeedback");
                        _logger.LogInformation("Set feedback UserId to: {UserId}", feedback.UserId);
                        
                        // Get current racerId if there's an active racer
                        var activeRacer = await _raceTeamService.GetActiveRacer();
                        if (activeRacer != null) {
                            feedback.RacerId = activeRacer.Id;
                            _logger.LogInformation("Set feedback RacerId to: {RacerId} for racer: {RacerName}", 
                                feedback.RacerId, activeRacer.Name);
                        } else {
                            feedback.RacerId = null;
                            _logger.LogInformation("No active racer found, RacerId set to null");
                        }
                    } else {
                        // User is not authenticated, leave UserId and RacerId as null/empty
                        feedback.UserId = null;
                        feedback.RacerId = null;
                        _logger.LogInformation("User not authenticated, UserId and RacerId set to null");
                    }
                } catch (Exception ex) {
                    // If we can't get user context, log the error but don't fail the feedback submission
                    _logger.LogWarning(ex, "Could not get user context for feedback, proceeding with anonymous feedback");
                    feedback.UserId = null;
                    feedback.RacerId = null;
                }
                
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding feedback");
                return false;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback) {
            try {
                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error updating feedback");
                return false;
            }
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes) {
            try {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
                feedback.ResolvedAt = DateTime.UtcNow;
                
                _context.Feedbacks.Update(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error resolving feedback");
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id) {
            try {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return false;

                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                return true;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting feedback");
                return false;
            }
        }

        public List<Feedback> GenerateFakeFeedbacks(int count = 3) {
            var faker = new Faker<Feedback>()
                .RuleFor(f => f.Id, f => 0)
                .RuleFor(f => f.UserId, f => f.Random.Guid().ToString())
                .RuleFor(f => f.RacerId, f => f.Random.Bool() ? f.Random.Int(1, 100) : (int?)null)
                .RuleFor(f => f.Name, f => f.Name.FullName())
                .RuleFor(f => f.Email, f => f.Internet.Email())
                .RuleFor(f => f.FeedbackType, f => f.PickRandom<FeedbackType>())
                .RuleFor(f => f.Subject, f => f.Lorem.Sentence(5))
                .RuleFor(f => f.Message, f => f.Lorem.Paragraphs(1))
                .RuleFor(f => f.BrowserInfo, f => f.Random.String2(20))
                .RuleFor(f => f.ContactConsent, f => f.Random.Bool())
                .RuleFor(f => f.SubmittedAt, f => f.Date.Recent())
                .RuleFor(f => f.IsResolved, f => f.Random.Bool())
                .RuleFor(f => f.AdminNotes, f => f.Lorem.Sentence(6))
                .RuleFor(f => f.ResolvedAt, (f, feedback) => feedback.IsResolved ? f.Date.Recent() : null);

            return faker.Generate(count);
        }
    }
}