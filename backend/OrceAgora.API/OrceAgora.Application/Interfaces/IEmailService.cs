namespace OrceAgora.Application.Interfaces;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string toEmail, string toName, string confirmUrl);
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetUrl);
}