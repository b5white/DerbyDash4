using DerbyDash.Data;

namespace DerbyDash.Services {
    public class StubFAQService : IFAQService {
        public Task<List<FAQ>> GetAllFAQsAsync() {
            // TODO: Implement API call to get FAQs
            return Task.FromResult(new List<FAQ>());
        }

        public Task<List<string>> GetCategoriesAsync() {
            // TODO: Implement API call to get categories
            return Task.FromResult(new List<string>());
        }

        public Task<List<FAQ>> GetFAQsByCategoryAsync(string category) {
            // TODO: Implement API call to get FAQs by category
            return Task.FromResult(new List<FAQ>());
        }
    }
}
