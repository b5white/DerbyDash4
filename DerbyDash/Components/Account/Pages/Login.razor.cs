using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Components.Account.Pages {
    public partial class Login {
        private string? errorMessage;
        private bool showPassword = false;

        [Inject]
        private UserManager<ApplicationUser> UserManager { get; set; } = null!;

        [Inject]
        private ILogger<Login> Logger { get; set; } = null!;

        [Inject]
        private NavigationManager NavManager { get; set; } = null!;

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private void TogglePasswordVisibility() {
            showPassword = !showPassword;
            // No need for StateHasChanged() here as Blazor handles UI updates for bound values on events
        }

        protected override async Task OnInitializedAsync() {
            // No HttpContext usage in interactive mode
            await Task.CompletedTask;
        }

        public async Task LoginUser() {
            Console.WriteLine("Logging in user.");

            // Clear any existing error message
            errorMessage = "";

            // Basic form validation check (though DataAnnotationsValidator handles this too)
            if (string.IsNullOrWhiteSpace(Input.Email) || string.IsNullOrWhiteSpace(Input.Password)) {
                errorMessage = "Error: Email and password are required.";
                return;
            }

            try {
                // First, find the user by email
                var user = await UserManager.FindByEmailAsync(Input.Email.Trim());

                if (user == null) {
                    errorMessage = "Error: Invalid login attempt. Please check your email and password.";
                    Logger.LogWarning("Failed login attempt for non-existent user: {Email}", Input.Email);
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
                    isPasswordValid = await UserManager.CheckPasswordAsync(user, Input.Password);
                }
                // --- END OF FAKE LOGIN MODIFICATION ---

                if (!isPasswordValid) {
                    // This message will now only show for non-test users with wrong passwords
                    errorMessage = "Error: Invalid login attempt. Please check your email and password.";
                    Logger.LogWarning("Failed login attempt (incorrect password) for user: {Email}", Input.Email);
                    return;
                }

                // Check if email is confirmed if required
                try {
                    if (UserManager.Options.SignIn.RequireConfirmedEmail && !await UserManager.IsEmailConfirmedAsync(user)) {
                        errorMessage = "Error: You must confirm your email before logging in.";
                        // Optionally, provide a link to resend confirmation
                        // Example: errorMessage += " <a href='/Account/ResendEmailConfirmation'>Resend confirmation</a>";
                        Logger.LogWarning("Login failed: Email not confirmed for user {Email}", Input.Email);
                        return;
                    }
                } catch (NotSupportedException) {
                    // Email confirmation not supported by the store, continue with login
                    Logger.LogInformation("Email confirmation feature not supported by the store for user {Email}", Input.Email);
                }

                // Check if account is locked out - skip if not supported
                try {
                    if (await UserManager.IsLockedOutAsync(user)) {
                        errorMessage = "Error: Account locked out. Please try again later or contact support.";
                        Logger.LogWarning("User account locked out: {Email}", Input.Email);
                        return;
                    }
                } catch (NotSupportedException) {
                    // Lockout not supported by the store, continue with login
                    Logger.LogInformation("User lockout feature not supported by the store for user {Email}", Input.Email);
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

                Logger.LogInformation("Redirecting user {Email} to ProcessLogin with ReturnUrl: {ReturnUrl}", Input.Email, returnUrl);
                NavManager.NavigateTo($"/Account/ProcessLogin?email={Uri.EscapeDataString(Input.Email)}&rememberMe={Input.RememberMe}&returnUrl={Uri.EscapeDataString(returnUrl)}", true);
            } catch (Exception ex) {
                errorMessage = $"Error: An unexpected error occurred during login attempt."; // Avoid exposing ex.Message directly to user
                Logger.LogError(ex, "Error during login attempt for user: {Email}", Input.Email);
            }
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