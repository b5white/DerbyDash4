using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
//using SendGrid;
//using SendGrid.Helpers.Mail;

namespace DerbyDash.Services {

    public class EmailSender: IEmailSender, IEmailSender<ApplicationUser> {
        private readonly ILogger<EmailSender> Logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                           ILogger<EmailSender> logger) {
            Options = optionsAccessor.Value;
            Logger = logger;
        }

        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
            => await SendEmailAsync(email, "Confirm your email",
            "<html lang=\"en\"><head></head><body>Please confirm your account by " +
            $"<a href='{confirmationLink}'>clicking here</a>.</body></html>");

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
            => await SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password by " +
            $"<a href='{resetLink}'>clicking here</a>.</body></html>");

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
            => await SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password " +
            $"using the following code:<br>{resetCode}</body></html>");


        public async Task SendEmailAsync(string toEmail, string subject, string message) {
            if (string.IsNullOrEmpty(Options.SendGridKey)) {
                Logger.LogError("Null SendGridKey");
                return;
            }
            await Execute(Options.SendGridKey, subject, message, toEmail);
        }

        public async Task Execute(string apiKey, string subject, string message, string toEmail) {
            //SendGridClient client = new SendGridClient(apiKey);
            //SendGridMessage msg = new SendGridMessage() {
            //    From = new EmailAddress(Options.SenderEmail, Options.SenderName),
            //    Subject = subject,
            //    PlainTextContent = message,
            //    HtmlContent = message
            //};
            //msg.AddTo(new EmailAddress(toEmail));

            //// Disable click tracking.
            //// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            //msg.SetClickTracking(false, false);
            //Response response = await client.SendEmailAsync(msg);
            //Logger.LogInformation(response.IsSuccessStatusCode
            //                       ? $"Email to {toEmail} queued successfully!"
            //                       : $"Failure Email to {toEmail}");
            await Task.CompletedTask; // Just to use 'await'
            return;
        }
    }
}