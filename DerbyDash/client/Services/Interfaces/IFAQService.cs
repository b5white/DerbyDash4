using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IFAQService {
        Task<List<FAQ>> GetAllFAQsAsync();
        Task<List<string>> GetCategoriesAsync();
        Task<List<FAQ>> GetFAQsByCategoryAsync(string category);
    }
}
