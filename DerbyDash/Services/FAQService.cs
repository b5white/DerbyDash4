using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IFAQService {
        Task<List<FAQ>> GetAllFAQsAsync();
        Task<List<string>> GetCategoriesAsync();
        Task<List<FAQ>> GetFAQsByCategoryAsync(string category);
    }

    public class FAQService: IFAQService {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<FAQ>? _cachedFAQs;

        public FAQService(IWebHostEnvironment webHostEnvironment) {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<FAQ>> GetAllFAQsAsync() {
            if (_cachedFAQs != null)
                return _cachedFAQs;

            _cachedFAQs = await LoadFAQsFromCsvAsync();
            return _cachedFAQs;
        }

        public async Task<List<string>> GetCategoriesAsync() {
            var faqs = await GetAllFAQsAsync();
            return faqs.Select(f => f.Category).Distinct().OrderBy(c => c).ToList();
        }

        public async Task<List<FAQ>> GetFAQsByCategoryAsync(string category) {
            var faqs = await GetAllFAQsAsync();
            return faqs.Where(f => f.Category == category).ToList();
        }

        private async Task<List<FAQ>> LoadFAQsFromCsvAsync() {
            try {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "data", "FAQ.csv");

                // Read the file manually to handle tab-delimited format
                var faqs = new List<FAQ>();
                var lines = await File.ReadAllLinesAsync(filePath);

                // Skip header
                for (int i = 1; i < lines.Length; i++) {
                    var line = lines[i];
                    var parts = line.Split('\t');

                    if (parts.Length >= 3) {
                        faqs.Add(new FAQ {
                            Category = parts[0].Trim(),
                            Question = parts[1].Trim(),
                            Answer = parts[2].Trim()
                        });
                    }
                }

                return faqs;
            } catch (Exception ex) {
                Console.WriteLine($"Error loading FAQ data: {ex.Message}");
                // Return an empty list in case of error
                return new List<FAQ>();
            }
        }
    }
}