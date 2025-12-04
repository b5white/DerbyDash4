using Blazorise.Captcha;
using DerbyDash.Components.Account;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages {
    public partial class Login {
        private string? errorMessage;

        [Inject]
        public required ApiAuthService AuthService { get; set; }

        [Inject]
        public required AuthenticationStateProvider AuthStateProvider { get; set; }

        private CustomAuthStateProvider CustomAuthStateProvider => (CustomAuthStateProvider)AuthStateProvider;

        [Inject]
        public required ILogger<Login> Logger { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }


        private Blazorise.Captcha.ReCaptcha.ReCaptcha captcha = default!;
        private bool canSubmit = false;
        private EditContext? editContext;
        private string siteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI"; // Google's test key
        private bool isSubmitting = false;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        protected override void OnInitialized() {
            base.OnInitialized();
            editContext = new EditContext(Input);
            editContext.OnFieldChanged += EditContext_OnFieldChanged;
            editContext.OnValidationStateChanged += EditContext_OnValidationStateChanged;
            UpdateCanSubmit();
        }

        public async Task LoginUser() {
            if (isSubmitting) return;
            isSubmitting = true;

            try {
                Console.WriteLine("Logging in user.");

                // Check reCAPTCHA first
                if (!captcha.State.Valid || string.IsNullOrEmpty(captcha.State.Response)) {
                    errorMessage = "Error: Please complete the reCAPTCHA verification.";
                    isSubmitting = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // Clear any existing error message
                errorMessage = "";
                string email = Input.Email.Trim();
                string password = Input.Password.Trim();

                // Basic form validation check
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
                    errorMessage = "Error: Email and password are required.";
                    isSubmitting = false;
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // Get reCAPTCHA token
                string recaptchaToken = captcha.State.Response;

                // Call API to login (server will verify reCAPTCHA)
                var response = await AuthService.LoginAsync(email, password, recaptchaToken);

                if (response?.Success == true && response.Data != null) {
                    // Notify auth state provider of successful login
                    CustomAuthStateProvider.NotifyUserAuthentication(response.Data.AccessToken);

                    // Reset reCAPTCHA after successful login
                    if (captcha != null) {
                        await captcha.Reset();
                    }

                    // Redirect to return URL or home
                    var returnUrl = ReturnUrl ?? "/";
                    if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)) {
                        Logger.LogWarning("Invalid ReturnUrl provided: {ReturnUrl}. Redirecting to default.", returnUrl);
                        returnUrl = "/";
                    }

                    Logger.LogInformation("User {Email} logged in successfully", email);
                    NavManager.NavigateTo(returnUrl, forceLoad: true);
                } else {
                    errorMessage = response?.Message ?? "Error: Invalid login attempt. Please check your email and password.";
                    Logger.LogWarning("Failed login attempt for user: {Email}", email);
                    
                    // Reset reCAPTCHA on failure
                    if (captcha != null) {
                        await captcha.Reset();
                    }
                    Input.Captcha = false;
                    editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
                    UpdateCanSubmit();
                    
                    isSubmitting = false;
                    await InvokeAsync(StateHasChanged);
                }
            } catch (Exception ex) {
                errorMessage = "Error: An unexpected error occurred during login attempt.";
                Logger.LogError(ex, "Error during login attempt for user: {Email}", Input.Email);
                
                // Reset reCAPTCHA on error
                if (captcha != null) {
                    await captcha.Reset();
                }
                Input.Captcha = false;
                editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
                UpdateCanSubmit();
                
                isSubmitting = false;
                await InvokeAsync(StateHasChanged);
            }
        }


        private void Solved(CaptchaState state) {
            Logger.LogInformation($"Captcha Success: {state}");
            Logger.LogInformation($"Captcha State - Valid: {state.Valid}, Response: {state.Response}");
            if (state.Valid && !string.IsNullOrEmpty(state.Response)) {
                Logger.LogInformation("Captcha response token captured.");
                Input.Captcha = true;
            } else {
                Logger.LogWarning("Captcha was not successfully solved.");
                Input.Captcha = false;
            }
            // Notify EditContext manually
            editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
            UpdateCanSubmit();
            InvokeAsync(StateHasChanged);
        }

        private void Expired() {
            Logger.LogInformation("Captcha expired.");
            Input.Captcha = false;
            // Notify EditContext manually
            editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
            UpdateCanSubmit();
            InvokeAsync(StateHasChanged);
        }

        private Task<bool> Validate(CaptchaState state) {
            Logger.LogInformation($"Captcha Validate: {state}");
            Logger.LogInformation($"Captcha Validate - Valid: {state.Valid}, Response: {state.Response}");
            // Only check if token exists, don't call Google API here
            Input.Captcha = !string.IsNullOrEmpty(state.Response);
            Logger.LogInformation($"Input.Captcha set to: {Input.Captcha}");
            // Notify EditContext manually
            editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
            UpdateCanSubmit();
            InvokeAsync(StateHasChanged);
            return Task.FromResult(Input.Captcha);
        }

        private async Task Reset() {
            if (captcha != null) {
                await captcha.Reset();
            }
            Input.Captcha = false;
            // Notify EditContext manually
            editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
            UpdateCanSubmit();
            await InvokeAsync(StateHasChanged);
        }

        private void EditContext_OnFieldChanged(object? sender, FieldChangedEventArgs e) {
            // Trigger validation on each field change
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

        public class InputModel {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; } = false;

            [Required(ErrorMessage = "Please verify that you are human.")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "Please verify that you are not a robot.")]
            [Display(Name = "I'm not a robot")]
            public bool Captcha { get; set; } = false;
        }

    }
}
