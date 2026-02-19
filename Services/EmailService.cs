using System.Net;
using System.Net.Mail;

namespace vault_backend.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp)
    {
        var smtpHost = _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
        if (!int.TryParse(_configuration["Email:SmtpPort"], out var smtpPort))
            throw new InvalidOperationException("Email:SmtpPort is missing or not a valid integer.");
        var senderEmail = _configuration["Email:SenderEmail"]
            ?? throw new InvalidOperationException("Email:SenderEmail is not configured.");
        var senderPassword = _configuration["Email:SenderPassword"]
            ?? throw new InvalidOperationException("Email:SenderPassword is not configured.");
        var senderName = _configuration["Email:SenderName"] ?? "Vault App";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = "Your Vault OTP Code",
            Body = $"Your one-time password is: {otp}\n\nThis code expires in 10 minutes.",
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
            throw;
        }
    }
}
