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
        private CustomAuthStateProvider AuthStateProvider { get; set; } = null!;

        [Inject]
        public required IConfiguration configuration { get; set; }

        [Inject] IHttpClientFactory HttpClientFactory { get; set; } = default!;

        private Blazorise.Captcha.ReCaptcha.ReCaptcha captcha = default!;
        private bool canSubmit = false;
        private EditContext? editContext;
        private string siteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI"; // Google's test key

        [SupplyParameterFromForm]
        protected InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private string Message = "";

        protected override void OnInitialized() {
            base.OnInitialized();
            var stepsHelper = new RegistrationSteps { CurrentStep = "register" };
            registrationStepsList = stepsHelper.GetRegistrationSteps();
            editContext = new EditContext(Input);
            editContext.OnFieldChanged += EditContext_OnFieldChanged;
            editContext.OnValidationStateChanged += EditContext_OnValidationStateChanged;
            UpdateCanSubmit();
        }

        public async Task RegisterUser(EditContext editContext) {
            try {
                if (captcha.State.Valid) {
                    var isHuman = await VerifyWithGoogle();
                    canSubmit = false;
                    await captcha!.Reset();
                    if (!isHuman) {
                        Logger.LogInformation("reCaptcha failed for {email}", Input.Email);
                        return;
                    }
                    Message = "";
                    var user = CreateUser();
                    string email = Input.Email.Trim();
                    await UserStore.SetUserNameAsync(user, email, CancellationToken.None);
                    var emailStore = GetEmailStore();
                    await emailStore.SetEmailAsync(user, email, CancellationToken.None);
                    IdentityResult result = await UserManager.CreateAsync(user, Input.Password);

                    if (!result.Succeeded) {
                        Message = result.Errors.First().Description;
                        StateHasChanged();
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
                    await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
                    Logger.LogInformation("Redirecting to RegisterConfirmation page after registration");
                    var confirmationUrl = $"Account/RegisterConfirmation?email={Uri.EscapeDataString(email)}&rememberMe={Input.RememberMe}";
                    await InvokeAsync(() => NavManager.NavigateTo(confirmationUrl, forceLoad: true));
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error during registration for {Email}", Input.Email);
                Message = "An unexpected error occurred. Please try again.";
                StateHasChanged();
            }
        }


        private void Solved(CaptchaState state) {
            Logger.LogInformation($"Captcha Success: {state}");
            if (state.Valid && !string.IsNullOrEmpty(state.Response)) {
                Logger.LogInformation("Captcha response token captured.");
            } else {
                Logger.LogWarning("Captcha was not successfully solved.");
        }
            StateHasChanged();
        }

        private void Expired() {
            Logger.LogDebug("Captcha Expired");
            canSubmit = false;
            Input.Captcha = false;
            UpdateCanSubmit();
        }

        private async Task Reset() {
            await captcha.Reset();
            Input.Captcha = false;
            UpdateCanSubmit();
        }

        private Task<bool> Validate(CaptchaState state) {
            // Only check if token exists, don't call Google API here
            Input.Captcha = !string.IsNullOrEmpty(state.Response);
            // Notify EditContext manually
            editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
            return Task.FromResult(Input.Captcha);
        }

        private async Task<bool> VerifyWithGoogle() {
            Logger.LogInformation("Captcha VerifyWithGoogle");
            CaptchaState state = captcha.State;
            Logger.LogInformation("Captcha state - Valid: {Valid}, Response: {HasResponse}", state.Valid, !string.IsNullOrEmpty(state.Response));
            
            // Check if we have a response token
            if (string.IsNullOrEmpty(state.Response)) {
                Logger.LogWarning("No captcha response token");
                return false;
            }

            try {
                var content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("secret", "6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe"), // Google's test secret key
                    // new KeyValuePair<string, string>("secret", configuration.GetValue<string>("ReCaptcha:SecretKey") ?? ""),
                    new KeyValuePair<string, string>("response", state.Response),
                    // Optional: Add user's IP for additional validation
                    // new KeyValuePair<string, string>("remoteip", GetUserIP())
                });

                var httpClient = HttpClientFactory.CreateClient("ReCaptcha");
                Logger.LogInformation("Sending reCAPTCHA verification request to Google");
                var response = await httpClient.PostAsync("siteverify", content);

                if (!response.IsSuccessStatusCode) {
                    Logger.LogWarning($"reCAPTCHA API call failed: {response.StatusCode}");
                    return false;
                }

                var result = await response.Content.ReadAsStringAsync();
                Logger.LogInformation("reCAPTCHA API response: {Response}", result);
                
                var googleResponse = JsonSerializer.Deserialize<GoogleResponse>(result, new JsonSerializerOptions() {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var isValid = googleResponse?.Success ?? false;
                Logger.LogInformation("reCAPTCHA validation result: {IsValid}", isValid);

                if (!isValid && googleResponse?.ErrorCodes?.Length > 0) {
                    Logger.LogWarning($"reCAPTCHA validation failed: {string.Join(", ", googleResponse.ErrorCodes)}");
                }

                return isValid;
            } catch (Exception ex) {
                Logger.LogWarning($"reCAPTCHA validation error: {ex.Message}");
                return false; // Fail secure
            }
        }

        private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e) {
            // Optionally, trigger validation on each field change
            editContext?.Validate();
        }

        private void EditContext_OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) {
            UpdateCanSubmit();
            StateHasChanged(); // Ensure UI updates
        }

        private void UpdateCanSubmit() {
            // canSubmit is true only if all fields are valid
            canSubmit = !(editContext?.GetValidationMessages().Any() ?? true);
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

            [Required(ErrorMessage = "Please verify that you are human.")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Please verify that you are not a robot.")]
            [Display(Name = "I'm not a robot")]
            public bool Captcha { get; set; } = false;
        }
    }
}