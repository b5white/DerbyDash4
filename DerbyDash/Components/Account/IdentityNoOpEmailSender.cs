using DerbyDash.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DerbyDash.Components.Account {
    // Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
    internal sealed class IdentityNoOpEmailSender: IEmailSender<ApplicationUser> {

        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
            => await emailSender.SendEmailAsync(email, "Confirm your Derby Dash Account", 
                $@"<html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
            .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; border-radius: 5px; }}
            .content {{ padding: 20px; background-color: #f9f9f9; border-radius: 5px; margin-top: 20px; }}
            .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
            .footer {{ margin-top: 20px; font-size: 12px; color: #777; text-align: center; }}
        </style>
    </head>
    <body>
        <div class='header'>
            <h2>Derby Dash Account Confirmation</h2>
        </div>
        <div class='content'>
            <p>Hello {user.UserName},</p>
            <p>Thank you for registering with Derby Dash! We're excited to have you join our community.</p>
            <p>To complete your registration and activate your account, please click the button below:</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>Confirm My Account</a>
            </p>
            <p>If the button doesn't work, you can also copy and paste the following link into your browser:</p>
            <p>{confirmationLink}</p>
            <p>This link will expire in 24 hours for security reasons.</p>
        </div>
        <div class='footer'>
            <p>© Derby Dash. All rights reserved.</p>
            <p>If you didn't create this account, please ignore this email.</p>
        </div>
    </body>
    </html>");

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
            => await emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
            => await emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
    }
}
