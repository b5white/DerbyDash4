using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace DerbyDash.Components.Account.Pages {
    public partial class Register: ComponentBase {
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

        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = new();

        [SupplyParameterFromQuery]
        private string? ReturnUrl { get; set; }

        private string? Message => identityErrors is null ? null : $"Error: {string.Join(", ", identityErrors.Select(error => error.Description))}";

        public async Task RegisterUser(EditContext editContext) {
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
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });
            if (UserManager.Options.SignIn.RequireConfirmedAccount) {
                await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
                Logger.LogInformation("Email confirmation required - redirecting to RegisterConfirmation page");
            } else {
                Logger.LogInformation("Email confirmation not required - automatically signing in user");

                // Automatically sign in the user when email confirmation is disabled
                await SignInManager.SignInAsync(user, isPersistent: false);
                AuthStateProvider.NotifyUserLogin();
                Logger.LogInformation("User automatically signed in: {Email}", email);
            }

            // Always redirect to the RegisterConfirmation page after successful registration
            var confirmationUrl = $"Account/RegisterConfirmation?email={Uri.EscapeDataString(email)}";
            RedirectManager.RedirectTo(confirmationUrl);
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

        private sealed class InputModel {
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
        }
    }
}