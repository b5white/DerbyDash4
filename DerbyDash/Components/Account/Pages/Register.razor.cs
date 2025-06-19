using Blazorise.Captcha;
using DerbyDash.Components.Shared;
using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static DerbyDash.Components.Shared.RegistrationProgress;

namespace DerbyDash.Components.Account.Pages {
    public partial class Register: ComponentBase {
        private List<RegistrationStep> registrationStepsList = new();

        private IEnumerable<IdentityError>? identityErrors;

        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; } = null!;

        [Inject]
        private IUserStore<ApplicationUser> UserStore { get; set; } = null!;

        [Inject]
        private SignInManager<ApplicationUser> SignInManager { get; set; } = null!;

        [Inject]
        private IEmailSender<ApplicationUser> EmailSender { get; set; } = null!;

        [Inject]
        private ILogger<Register> Logger { get; set; } = null!;

        [Inject]
        private NavigationManager NavManager { get; set; } = null!;

        [Inject]
        private IdentityRedirectManager RedirectManager { get; set; } = null!;

        [Inject]
        private CustomAuthStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject] IHttpClientFactory HttpClientFactory { get; set; } = default!;

        private Blazorise.Captcha.ReCaptcha.ReCaptcha captcha = default!;
        private bool canSubmit = false;

        [SupplyParameterFromForm]
        protected InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        protected override void OnInitialized() {
            base.OnInitialized();
            var stepsHelper = new RegistrationSteps { CurrentStep = "register" };
            registrationStepsList = stepsHelper.GetRegistrationSteps();
        }

        public async Task RegisterUser(EditContext editContext) {
            if (captcha.State.Valid) {

                var isHuman = await VerifyWithGoogle();
                if (!isHuman) {
                    canSubmit = false;
                    await captcha?.Reset(); // Force reCAPTCHA reset
                    return;
                }
                var user = CreateUser();
                string email = Input.Email.Trim();
                await UserStore.SetUserNameAsync(user, email, CancellationToken.None);
                var emailStore = GetEmailStore();
                await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                var result = await UserManager.CreateAsync(user, Input.Password);

                if (!result.Succeeded) {
                    identityErrors = result.Errors;
                    return;
                }

                var userId = await UserManager.GetUserIdAsync(user);
                Logger.LogInformation("User created a new account with password. {email} {userId}", email, userId);

				var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
				code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
				var callbackUrl = NavManager.GetUriWithQueryParameters(
					NavManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
					new Dictionary<string, object?> {
						["userId"] = userId,
						["code"] = code,
						["returnUrl"] = ReturnUrl,
						["rememberMe"] = Input.RememberMe
					});
				if (UserManager.Options.SignIn.RequireConfirmedAccount) {
					await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
					Logger.LogInformation("Email confirmation required - redirecting to RegisterConfirmation page");
				} else {
					Logger.LogInformation("Email confirmation not required - automatically signing in user");

					// Automatically sign in the user when email confirmation is disabled
					await SignInManager.SignInAsync(user, isPersistent: Input.RememberMe);
					AuthStateProvider.NotifyUserLogin();
					Logger.LogInformation("User automatically signed in: {Email}", email);
				}

				// Always redirect to the RegisterConfirmation page after successful registration
				var confirmationUrl = $"Account/RegisterConfirmation?email={Uri.EscapeDataString(email)}&rememberMe={Input.RememberMe}";
				RedirectManager.RedirectTo(confirmationUrl);
			}
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
            Logger.LogInformation("Captcha Validate");
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

        private ApplicationUser CreateUser() {
            try {
                return Activator.CreateInstance<ApplicationUser>();
            } catch {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore() {
            if (!UserManager.SupportsUserEmail) {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)UserStore;
        }

        public sealed class InputModel {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";

            [Display(Name = "Keep me logged in with cookies")]
            public bool RememberMe { get; set; } = true;
        }
    }
}