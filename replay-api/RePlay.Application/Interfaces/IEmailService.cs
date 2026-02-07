namespace RePlay.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string userName, string verificationCode);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetToken);
    Task SendTradeApprovedAsync(string toEmail, string userName, string toyName);
    Task SendReturnApprovedAsync(string toEmail, string userName, string toyName);
    Task SendReturnRejectedAsync(string toEmail, string userName, string toyName, string reason);
    Task SendNewMessageNotificationAsync(string toEmail, string userName, string senderName);
}
