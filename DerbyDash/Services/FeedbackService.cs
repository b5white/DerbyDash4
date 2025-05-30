using Bogus;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public class FeedbackService {

        private List<Feedback> Feedbacks;
        public FeedbackService() {
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

        public async Task<bool> AddFeedbackAsync(Feedback feedback) {
            try {
                feedback.SubmittedAt = DateTime.UtcNow;
                feedback.IsResolved = false;
                // Set optional foreign keys to null if not provided
                // These can be populated later if user authentication is available
                feedback.RacerId = 0;
                feedback.UserId = "";
                Feedbacks.Add(feedback);
                return await Task.FromResult(true);
            } catch (Exception ex) {
                Console.WriteLine($"Error adding feedback: {ex.Message}");
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
            try {
                var feedback = await GetFeedbackByIdAsync(id);
                if (feedback == null)
                    return false;

                feedback.IsResolved = true;
                feedback.AdminNotes = adminNotes;
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