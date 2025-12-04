using Blazorise.Captcha;
using DerbyDash.Components.Account;
using DerbyDash.Components.Shared;
using DerbyDash.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;
using static DerbyDash.Components.Shared.RegistrationProgress;

namespace DerbyDash.Components.Account.Pages {
    public partial class Register : ComponentBase {
        private List<RegistrationStep> registrationStepsList = new();

        [Inject]
        public required ApiAuthService AuthService { get; set; }

        [Inject]
        public required AuthenticationStateProvider AuthStateProvider { get; set; }

        private CustomAuthStateProvider CustomAuthStateProvider => (CustomAuthStateProvider)AuthStateProvider;

        [Inject]
        public required ILogger<Register> Logger { get; set; }

        [Inject]
        public required NavigationManager NavManager { get; set; }


        private Blazorise.Captcha.ReCaptcha.ReCaptcha captcha = default!;
        private bool canSubmit = false;
        private EditContext? editContext;
        private string siteKey = "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI"; // Google's test key
        private bool isSubmitting = false;

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
            if (isSubmitting) return;
            isSubmitting = true;

            try {
                // Check reCAPTCHA first
                if (!captcha.State.Valid || string.IsNullOrEmpty(captcha.State.Response)) {
                    Message = "Error: Please complete the reCAPTCHA verification.";
                    isSubmitting = false;
                    StateHasChanged();
                    return;
                }

                Message = "";
                string email = Input.Email.Trim();
                string password = Input.Password.Trim();
                string confirmPassword = Input.ConfirmPassword.Trim();

                // Basic validation
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) {
                    Message = "Error: Email and password are required.";
                    isSubmitting = false;
                    StateHasChanged();
                    return;
                }

                if (password != confirmPassword) {
                    Message = "Error: Password and confirmation password do not match.";
                    isSubmitting = false;
                    StateHasChanged();
                    return;
                }

                // Get reCAPTCHA token
                string recaptchaToken = captcha.State.Response;

                // Call API to register (server will verify reCAPTCHA)
                var response = await AuthService.RegisterAsync(email, password, confirmPassword, recaptchaToken);

                if (response?.Success == true && response.Data != null) {
                    // Notify auth state provider of successful registration/login
                    CustomAuthStateProvider.NotifyUserAuthentication(response.Data.AccessToken);

                    // Reset reCAPTCHA after successful registration
                    if (captcha != null) {
                        await captcha.Reset();
                    }

                    Logger.LogInformation("User registered successfully: {email}", email);

                    // Redirect to return URL or home
                    var returnUrl = ReturnUrl ?? "/";
                    if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)) {
                        Logger.LogWarning("Invalid ReturnUrl provided: {ReturnUrl}. Redirecting to default.", returnUrl);
                        returnUrl = "/";
                    }

                    NavManager.NavigateTo(returnUrl, forceLoad: true);
                } else {
                    // Handle registration errors
                    if (response?.Errors != null && response.Errors.Count > 0) {
                        Message = string.Join(" ", response.Errors);
                    } else {
                        Message = response?.Message ?? "Error: Registration failed. Please try again.";
                    }
                    Logger.LogWarning("Registration failed for user: {Email}. Errors: {Errors}", email, response?.Errors);
                    
                    // Reset reCAPTCHA on failure
                    if (captcha != null) {
                        await captcha.Reset();
                    }
                    Input.Captcha = false;
                    editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
                    UpdateCanSubmit();
                    
                    isSubmitting = false;
                    StateHasChanged();
                }
            } catch (Exception ex) {
                Logger.LogError(ex, "Error during registration for {Email}", Input.Email);
                Message = "An unexpected error occurred. Please try again.";
                
                // Reset reCAPTCHA on error
                if (captcha != null) {
                    await captcha.Reset();
                }
                Input.Captcha = false;
                editContext!.NotifyFieldChanged(editContext.Field(nameof(Input.Captcha)));
                UpdateCanSubmit();
                
                isSubmitting = false;
                StateHasChanged();
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
            UpdateCanSubmit();
            InvokeAsync(StateHasChanged);
        }

        private void Expired() {
            Logger.LogDebug("Captcha Expired");
            canSubmit = false;
            Input.Captcha = false;
            UpdateCanSubmit();
            InvokeAsync(StateHasChanged);
        }

        private async Task Reset() {
            await captcha.Reset();
            Input.Captcha = false;
            UpdateCanSubmit();
        }

        private Task<bool> Validate(CaptchaState state) {
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
