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
            => await SendEmailAsync(email, "Confirm your Derby Dash Account",
            $@"<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px; }}
        .content {{ padding: 20px; background-color: #f9f9f9; border-radius: 5px; margin-top: 20px; }}
        .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #777; text-align: center; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h2>Derby Dash Account Confirmation</h2>
    </div>
    <div class=""content"">
        <p>Hello {user.UserName},</p>
        <p>Thank you for registering with Derby Dash! We're excited to have you join our community.</p>
        <p>To complete your registration and activate your account, please click the button below:</p>
        <p style=""text-align: center;"">
            <a href=""{confirmationLink}"" class=""button"">Confirm My Account</a>
        </p>
        <p>If the button doesn't work, you can also copy and paste the following link into your browser:</p>
        <p>{confirmationLink}</p>
        <p>This link will expire in 24 hours for security reasons.</p>
    </div>
    <div class=""footer"">
        <p>© Derby Dash. All rights reserved.</p>
        <p>If you didn't create this account, please ignore this email.</p>
    </div>
</body>
</html>");

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