using DerbyDash.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace DerbyDash.Components.Account.Pages.Manage {
    public partial class Email {
        private string? message;
        private ApplicationUser user = default!;
        private string? email;
        private bool isEmailConfirmed;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        [Inject]
        public UserManager<ApplicationUser> UserManager { get; set; } = default!;
        [Inject]
        internal IEmailSender<ApplicationUser> EmailSender { get; set; } = default!;
        //[Inject]
        //internal IdentityUserAccessor UserAccessor { get; set; } = default!;
        [Inject]
        internal NavigationManager NavManager { get; set; } = default!;
        [Inject]
        public ILogger<ChangePassword> Logger { get; set; } = default!;

        [SupplyParameterFromForm(FormName = "change-email")]
        private InputModel Input { get; set; } = new();

        protected override async Task OnInitializedAsync() {
            //user = await UserAccessor.GetRequiredUserAsync(HttpContext);
            //email = await UserManager.GetEmailAsync(user);
            //isEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user);

            //Input.NewEmail ??= email;
        }

        private async Task OnValidSubmitAsync() {
            if (Input.NewEmail is null || Input.NewEmail == email) {
                message = "Your email is unchanged.";
                return;
            }

            string userId = await UserManager.GetUserIdAsync(user);
            string code = await UserManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = NavManager.GetUriWithQueryParameters(
                NavManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["email"] = Input.NewEmail, ["code"] = code });

            await EmailSender.SendConfirmationLinkAsync(user, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Confirmation link to change email sent. Please check your email.";
        }

        private async Task OnSendEmailVerificationAsync() {
            if (email is null) {
                return;
            }

            string userId = await UserManager.GetUserIdAsync(user);
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = NavManager.GetUriWithQueryParameters(
                NavManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

            await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));

            message = "Verification email sent. Please check your email.";
        }

        private sealed class InputModel {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string? NewEmail { get; set; }
        }
    }
}