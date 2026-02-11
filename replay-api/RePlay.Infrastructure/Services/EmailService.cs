using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RePlay.Application.Interfaces;

namespace RePlay.Infrastructure.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(string toEmail, string userName, string verificationCode)
    {
        var subject = "Verify your RePlay account";
        var body = $@"
            <h2>Welcome to RePlay, {userName}!</h2>
            <p>Thank you for registering. Please use the following code to verify your email address:</p>
            <h1 style='color: #4F46E5; letter-spacing: 5px;'>{verificationCode}</h1>
            <p>This code expires in 15 minutes.</p>
            <p>If you didn't create an account with RePlay, you can safely ignore this email.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetToken)
    {
        var subject = "Reset your RePlay password";
        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password. Use the following code to reset it:</p>
            <h1 style='color: #4F46E5; letter-spacing: 5px;'>{resetToken}</h1>
            <p>This code expires in 15 minutes.</p>
            <p>If you didn't request a password reset, you can safely ignore this email.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendTradeApprovedAsync(string toEmail, string userName, string toyName)
    {
        var subject = "Your trade has been approved!";
        var body = $@"
            <h2>Great news, {userName}!</h2>
            <p>Your trade for <strong>{toyName}</strong> has been approved.</p>
            <p>Visit your dashboard to see the details.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendReturnApprovedAsync(string toEmail, string userName, string toyName)
    {
        var subject = "Your return has been approved";
        var body = $@"
            <h2>Return Approved</h2>
            <p>Hi {userName},</p>
            <p>Your return of <strong>{toyName}</strong> has been approved and processed.</p>
            <p>Thank you for keeping our toy library in great condition!</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendReturnRejectedAsync(string toEmail, string userName, string toyName, string reason)
    {
        var subject = "Return requires attention";
        var body = $@"
            <h2>Return Update</h2>
            <p>Hi {userName},</p>
            <p>Your return of <strong>{toyName}</strong> requires attention.</p>
            <p>Reason: {reason}</p>
            <p>Please check your messages for more details.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendNewMessageNotificationAsync(string toEmail, string userName, string senderName)
    {
        var subject = "You have a new message on RePlay";
        var body = $@"
            <h2>New Message</h2>
            <p>Hi {userName},</p>
            <p>You have received a new message from <strong>{senderName}</strong>.</p>
            <p>Log in to RePlay to read and respond.</p>
        ";

        await SendEmailAsync(toEmail, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        _logger.LogInformation(
            "Sending email to {ToEmail} with subject '{Subject}'",
            toEmail,
            subject
        );

        // Wrap content in a branded HTML template
        var fullHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8' /></head>
<body style='margin:0; padding:0; background-color:#f3f4f6; font-family:Arial,sans-serif;'>
  <div style='max-width:600px; margin:0 auto; padding:20px;'>
    <div style='background-color:#4F46E5; padding:20px; border-radius:8px 8px 0 0; text-align:center;'>
      <h1 style='color:#ffffff; margin:0; font-size:24px;'>RePlay</h1>
    </div>
    <div style='background-color:#ffffff; padding:30px; border-radius:0 0 8px 8px;'>
      {htmlBody}
    </div>
    <div style='text-align:center; padding:20px; color:#9ca3af; font-size:12px;'>
      <p>&copy; {DateTime.UtcNow.Year} RePlay. All rights reserved.</p>
    </div>
  </div>
</body>
</html>";

        if (string.IsNullOrEmpty(_settings.SmtpHost))
        {
            _logger.LogWarning("SMTP not configured. Email to {ToEmail} was logged but not sent.", toEmail);
            return;
        }

        try
        {
            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = fullHtml };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {ToEmail}.", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
        }
    }
}
