using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DerbyDash.Pages {
    public class AboutModel : PageModel {
        private readonly ILogger<AboutModel> _logger;

        public AboutModel(ILogger<AboutModel> logger) {
            _logger = logger;
        }

        public string? Location { get; set; }

        // Set to true to show debug information, false for production
        public bool ShowDebugInfo { get; set; } = false;

        public string WelcomeMessage => Location?.ToLower() switch {
            "iowa" => "Welcome, Iowa Home School Conference Attendees!",
            "arizona" => "Welcome, Arizona Home School Conference Attendees!",
            _ => "Welcome!"
        };

        public void OnGet() {
            // Parse query string parameters
            if (Request.Query.TryGetValue("location", out var locationParam)) {
                Location = locationParam.ToString();
            }

            // Log the location parameter for debugging
            _logger.LogInformation("About page initialized with Location: {Location}", Location ?? "null");
        }
    }
}

