using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
//using SendGrid;
//using SendGrid.Helpers.Mail;

namespace DerbyDash.Services {

    public class EmailSender: IEmailSender, IEmailSender<ApplicationUser> {
        private readonly ILogger _logger;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                           ILogger<EmailSender> logger) {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
            => SendEmailAsync(email, "Confirm your email",
            "<html lang=\"en\"><head></head><body>Please confirm your account by " +
            $"<a href='{confirmationLink}'>clicking here</a>.</body></html>");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
            => SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password by " +
            $"<a href='{resetLink}'>clicking here</a>.</body></html>");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
            => SendEmailAsync(email, "Reset your password",
            "<html lang=\"en\"><head></head><body>Please reset your password " +
            $"using the following code:<br>{resetCode}</body></html>");


        public async Task SendEmailAsync(string toEmail, string subject, string message) {
            if (string.IsNullOrEmpty(Options.SendGridKey)) {
                _logger.LogError("Null SendGridKey");
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
            //_logger.LogInformation(response.IsSuccessStatusCode
            //                       ? $"Email to {toEmail} queued successfully!"
            //                       : $"Failure Email to {toEmail}");
        }
    }
}