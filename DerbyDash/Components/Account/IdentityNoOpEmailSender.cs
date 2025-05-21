using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DerbyDash.Components.Account {
    // Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
    internal sealed class IdentityNoOpEmailSender: IEmailSender<ApplicationUser> {

        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
            => await emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
            => await emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
            => await emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
