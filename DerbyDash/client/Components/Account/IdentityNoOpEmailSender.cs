using DerbyDash.Data;

namespace DerbyDash.Components.Account {
    /// <summary>
    /// Client-side no-op email sender (emails are handled server-side via API)
    /// </summary>
    internal sealed class IdentityNoOpEmailSender {
        // Email sending is handled server-side via API, so this is a no-op on the client
        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) {
            return Task.CompletedTask;
        }
    }
}
