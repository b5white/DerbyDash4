using DerbyDash.Data;
using Microsoft.Extensions.Logging;

namespace DerbyDash.Services {
    public interface IFAQService {
        Task<List<FAQ>> GetAllFAQsAsync();
        Task<List<string>> GetCategoriesAsync();
        Task<List<FAQ>> GetFAQsByCategoryAsync(string category);
    }

    public class FAQService: IFAQService {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FAQService> _logger;
        private List<FAQ>? _cachedFAQs;

        public FAQService(IWebHostEnvironment webHostEnvironment, ILogger<FAQService> logger) {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
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
                // Try multiple paths: WebRootPath, ContentRootPath/wwwroot, and ContentRootPath
                var possiblePaths = new List<string>();
                
                if (!string.IsNullOrEmpty(_webHostEnvironment.WebRootPath)) {
                    possiblePaths.Add(Path.Combine(_webHostEnvironment.WebRootPath, "data", "FAQ.csv"));
                }
                
                if (!string.IsNullOrEmpty(_webHostEnvironment.ContentRootPath)) {
                    possiblePaths.Add(Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "data", "FAQ.csv"));
                    possiblePaths.Add(Path.Combine(_webHostEnvironment.ContentRootPath, "..", "wwwroot", "data", "FAQ.csv"));
                    possiblePaths.Add(Path.Combine(_webHostEnvironment.ContentRootPath, "data", "FAQ.csv"));
                }

                string? filePath = null;
                foreach (var path in possiblePaths) {
                    var normalizedPath = Path.GetFullPath(path);
                    if (File.Exists(normalizedPath)) {
                        filePath = normalizedPath;
                        _logger.LogInformation("Found FAQ.csv at: {FilePath}", filePath);
                        break;
                    }
                }

                if (filePath == null) {
                    _logger.LogError("FAQ.csv not found in any of the following paths: {Paths}", string.Join(", ", possiblePaths));
                    return new List<FAQ>();
                }

                // Read the file manually to handle tab-delimited format
                var faqs = new List<FAQ>();
                var lines = await File.ReadAllLinesAsync(filePath);

                // Skip header
                for (int i = 1; i < lines.Length; i++) {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split('\t');

                    if (parts.Length >= 3) {
                        faqs.Add(new FAQ {
                            Category = parts[0].Trim(),
                            Question = parts[1].Trim(),
                            Answer = parts[2].Trim()
                        });
                    }
                }

                _logger.LogInformation("Loaded {Count} FAQs from {FilePath}", faqs.Count, filePath);
                return faqs;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error loading FAQ data");
                // Return an empty list in case of error
                return new List<FAQ>();
            }
        }
    }
}