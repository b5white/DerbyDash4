using Blazorise.Captcha;
using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DerbyDash.Components.Account.Pages {
    public partial class Login {
        private string? errorMessage;

        [Inject]
        public required UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        public required ILogger<Login> Logger { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }

        [Inject]
        internal IdentityRedirectManager RedirectManager { get; set; } = default!;

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject] 
        public required IHttpClientFactory HttpClientFactory { get; set; }

        private Blazorise.Captcha.ReCaptcha.ReCaptcha captcha = default!;
        private bool canSubmit = false;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        protected override async Task OnInitializedAsync() {
            // No HttpContext usage in interactive mode
            await Task.CompletedTask;
        }

        public async Task LoginUser() {
            Console.WriteLine("Logging in user.");

            // Check reCAPTCHA first
            if (!captcha.State.Valid) {
                errorMessage = "Error: Please complete the reCAPTCHA verification.";
                return;
            }

            var isHuman = await VerifyWithGoogle();
            if (!isHuman) {
                canSubmit = false;
                await captcha?.Reset(); // Force reCAPTCHA reset
                Logger.LogInformation("reCaptcha failed for {email}", Input.Email);
                errorMessage = "Error: reCAPTCHA verification failed. Please try again.";
                return;
            }

            // Clear any existing error message
            errorMessage = "";
            string email = Input.Email.Trim();
            string password = Input.Password.Trim();
            // Basic form validation check (though DataAnnotationsValidator handles this too)
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
                errorMessage = "Error: Email and password are required.";
                return;
            }

            try {
                // First, find the user by email
                var user = await UserManager.FindByEmailAsync(email);

                if (user == null) {
                    errorMessage = "Error: Invalid login attempt. Please check your email and password.";
                    Logger.LogWarning("Failed login attempt for non-existent user: {Email}", email);
                    return;
                }

                // --- FAKE LOGIN MODIFICATION ---
                bool isPasswordValid = false;
                // Check if the email is the special test email
                if (user.NormalizedEmail == "TEST@GMAIL.COM") // Use NormalizedEmail for reliable comparison
                {
                    Logger.LogWarning("Bypassing password check for test user: {Email}", Input.Email);
                    isPasswordValid = true; // Force password validation to pass
                } else {
                    // --- ORIGINAL PASSWORD CHECK ---
                    // Check the password for any other user
                    isPasswordValid = await UserManager.CheckPasswordAsync(user, password);
                }
                // --- END OF FAKE LOGIN MODIFICATION ---

                if (!isPasswordValid) {
                    // This message will now only show for non-test users with wrong passwords
                    errorMessage = "Error: Invalid login attempt. Please check your email and password.";
                    Logger.LogWarning("Failed login attempt (incorrect password) for user: {Email}", email);
                    return;
                }

                // Check if email is confirmed if required
                try {
                    if (UserManager.Options.SignIn.RequireConfirmedEmail && !await UserManager.IsEmailConfirmedAsync(user)) {
                        errorMessage = "Error: You must confirm your email before logging in.";
                        // Optionally, provide a link to resend confirmation
                        // Example: errorMessage += " <a href='/Account/ResendEmailConfirmation'>Resend confirmation</a>";
                        Logger.LogWarning("Login failed: Email not confirmed for user {Email}", email);
                        return;
                    }
                } catch (NotSupportedException) {
                    // Email confirmation not supported by the store, continue with login
                    Logger.LogInformation("Email confirmation feature not supported by the store for user {Email}", email);
                }

                // Check if account is locked out - skip if not supported
                try {
                    if (await UserManager.IsLockedOutAsync(user)) {
                        errorMessage = "Error: Account locked out. Please try again later or contact support.";
                        Logger.LogWarning("User account locked out: {Email}", email);
                        return;
                    }
                } catch (NotSupportedException) {
                    // Lockout not supported by the store, continue with login
                    Logger.LogInformation("User lockout feature not supported by the store for user {Email}", email);
                }

                // If we get here, the user is valid and can be signed in
                // Redirect to the ProcessLogin page which will handle the actual sign-in
                // Use RaceSetsMenu as the default redirect if no ReturnUrl is specified
                string defaultReturnUrl = "/RaceSetsMenu";
                var returnUrl = ReturnUrl ?? defaultReturnUrl;
                // Ensure ReturnUrl is a local URL before redirecting
                if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)) {
                    Logger.LogWarning("Invalid ReturnUrl provided: {ReturnUrl}. Redirecting to default.", returnUrl);
                    returnUrl = defaultReturnUrl;
                }

                Logger.LogInformation("Redirecting user {Email} to ProcessLogin with ReturnUrl: {ReturnUrl}", email, returnUrl);
                RedirectToAccountProcessLogin(email, Input.RememberMe, returnUrl);
            } catch (NavigationException) {
                throw;
            } catch (Exception ex) {
                errorMessage = $"Error: An unexpected error occurred during login attempt."; // Avoid exposing ex.Message directly to user
                Logger.LogError(ex, "Error during login attempt for user: {Email}", email);
            }
        }
        void RedirectToAccountProcessLogin(string email, bool rememberMe, string returnUrl) {
            var queryParams = new Dictionary<string, object?> {
                { "email", email },
                { "rememberMe", rememberMe },
                { "returnUrl", returnUrl }
            };
            RedirectManager!.RedirectToWParams("/Account/ProcessLogin", queryParams);
        }

        private void Solved(CaptchaState state) {
            Logger.LogInformation($"Captcha Success: {state}");
            if (state.Valid && !string.IsNullOrEmpty(state.Response)) {
                Logger.LogInformation("Captcha response token captured.");
                canSubmit = true;
            } else {
                Logger.LogWarning("Captcha was not successfully solved.");
                canSubmit = false;
            }
            StateHasChanged();
        }

        private void Expired() {
            Logger.LogDebug("Captcha Expired");
            canSubmit = false;
        }

        private async Task Reset() {
            await captcha.Reset();
        }

        private Task<bool> Validate(CaptchaState state) {
            // Only check if token exists, don't call Google API here
            return Task.FromResult(!string.IsNullOrEmpty(state.Response));
        }

        private async Task<bool> VerifyWithGoogle() {
            Logger.LogInformation("Captcha VerifyWithGoogle");
            CaptchaState state = captcha.State;
            // Check if we have a response token
            if (string.IsNullOrEmpty(state.Response)) {
                Logger.LogWarning("No captcha response token");
                return false;
            }

            try {
                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("secret", configuration.GetValue<string>("ReCaptcha:SecretKey") ?? ""),
                    new KeyValuePair<string, string>("response", state.Response),
                    // Optional: Add user's IP for additional validation
                    // new KeyValuePair<string, string>("remoteip", GetUserIP())
                });

                var httpClient = HttpClientFactory.CreateClient("ReCaptcha");
                var response = await httpClient.PostAsync("siteverify", content);

                if (!response.IsSuccessStatusCode) {
                    Logger.LogWarning($"reCAPTCHA API call failed: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonSerializer.Deserialize<GoogleResponse>(result, new JsonSerializerOptions() {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var isValid = googleResponse?.Success ?? false;

                if (!isValid && googleResponse?.ErrorCodes?.Length > 0) {
                    Logger.LogWarning($"reCAPTCHA validation failed: {string.Join(", ", googleResponse.ErrorCodes)}");
                }

                return isValid;
            } catch (Exception ex) {
                Logger.LogWarning($"reCAPTCHA validation error: {ex.Message}");
                return false; // Fail secure
            }
        }

        public class GoogleResponse {
            public bool Success { get; set; }
            public double Score { get; set; } //V3 only - The score for this request (0.0 - 1.0)
            public string Action { get; set; } = ""; //v3 only - An identifier
            public string Challenge_ts { get; set; } = "";
            public string Hostname { get; set; } = "";
            public string[] ErrorCodes { get; set; } = new string[0];
        }

        private sealed class InputModel {
            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Display(Name = "Use Cookies to stay logged in?")]
            public bool RememberMe { get; set; } = true;
            public string ReturnUrl { get; set; } = "";
        }
    }
}