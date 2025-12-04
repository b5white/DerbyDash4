using System.Text.Json;

namespace DerbyDash.Services {
    public interface IRecaptchaService {
        Task<bool> VerifyTokenAsync(string token);
    }

    public class RecaptchaService : IRecaptchaService {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RecaptchaService> logger) {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> VerifyTokenAsync(string token) {
            if (string.IsNullOrWhiteSpace(token)) {
                return false;
            }

            try {
                var secretKey = _configuration["ReCaptcha:SecretKey"];
                if (string.IsNullOrWhiteSpace(secretKey)) {
                    _logger.LogWarning("ReCAPTCHA secret key is not configured");
                    // In development, allow requests if secret key is not configured
                    return true;
                }

                var verifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}";

                var response = await _httpClient.GetAsync(verifyUrl);
                if (response.IsSuccessStatusCode) {
                    var json = await response.Content.ReadAsStringAsync();
                    
                    // Parse JSON manually to handle error-codes property name
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("success", out var successElement) && successElement.GetBoolean()) {
                        _logger.LogInformation("ReCAPTCHA verification successful");
                        return true;
                    } else {
                        // Extract error codes
                        var errorCodes = new List<string>();
                        if (root.TryGetProperty("error-codes", out var errorCodesElement) && errorCodesElement.ValueKind == JsonValueKind.Array) {
                            foreach (var errorCode in errorCodesElement.EnumerateArray()) {
                                errorCodes.Add(errorCode.GetString() ?? "Unknown");
                            }
                        }
                        
                        // Log the full response for debugging
                        _logger.LogWarning("ReCAPTCHA verification failed. Response: {Response}, Error codes: {ErrorCodes}", 
                            json,
                            errorCodes.Count > 0 ? string.Join(", ", errorCodes) : "None");
                        return false;
                    }
                } else {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("ReCAPTCHA API request failed with status code: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, "Error verifying reCAPTCHA token");
                return false;
            }
        }

    }
}

