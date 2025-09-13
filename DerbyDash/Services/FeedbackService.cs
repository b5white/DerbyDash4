using Bogus;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public class FeedbackService {
        private readonly IUserService _userService;
        private readonly IRaceTeamService _raceTeamService;
        private readonly ILogger<FeedbackService> _logger;

        private List<Feedback> Feedbacks;

        public FeedbackService(IUserService userService, IRaceTeamService raceTeamService, ILogger<FeedbackService> logger) {
            _userService = userService;
            _raceTeamService = raceTeamService;
            _logger = logger;
            Feedbacks = GenerateFakeFeedbacks(10);
        }

        public async Task<List<Feedback>> GetFeedbackSinceDateAsync(DateTime startDate) {
            var result = Feedbacks
                .Where(f => f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<Feedback>> GetFeedbackByTypeAsync(FeedbackType type, DateTime startDate) {
            var result = Feedbacks
                .Where(f => f.FeedbackType == type && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<Feedback>> GetFeedbackByStatusAsync(bool isResolved, DateTime startDate) {
            var result = Feedbacks
                .Where(f => f.IsResolved == isResolved && f.SubmittedAt >= startDate)
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int id) {
            var result = Feedbacks.Find(f => f.Id == id);
            return await Task.FromResult(result);
        }

        public async Task<List<Feedback>> GetFeedbackByUserIdAsync(string userId) {
            var result = Feedbacks.Where(f => f.UserId == userId).OrderByDescending(f => f.SubmittedAt).ToList();
            return await Task.FromResult(result);
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

                Feedbacks.Add(feedback);
                return await Task.FromResult(true);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding feedback");
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> UpdateFeedbackAsync(Feedback feedback) {
            try {
                //   Feedbacks.Update(feedback);
                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes) {
            return await ResolveFeedbackAsync(id, adminNotes, null, null);
        }

        public async Task<bool> ResolveFeedbackAsync(int id, string adminNotes, string? publicResponse = null, string? privateResponse = null) {
            try {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
                feedback.PublicResponse = publicResponse;
                feedback.PrivateResponse = privateResponse;
                feedback.ResolvedAt = DateTime.UtcNow;
                //   Feedbacks.Update(feedback);

                return true;
            } catch {
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id) {
            try {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return false;

                Feedbacks.Remove(feedback);
                return true;
            } catch {
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