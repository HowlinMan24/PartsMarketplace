using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string orderId, decimal totalAmount)
    {
        try
        {
            var subject = $"Order Confirmation - Order #{orderId}";
            var body = $@"
                <h2>Thank you for your order!</h2>
                <p>Your order has been confirmed.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><strong>Total Amount:</strong> {totalAmount:C}</p>
                <p>We will process your order and send you updates.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending order confirmation email to {Email}", toEmail);
        }
    }

    public async Task SendPaymentReceivedAsync(string toEmail, string orderId, decimal amount)
    {
        try
        {
            var subject = $"Payment Received - Order #{orderId}";
            var body = $@"
                <h2>Payment Received</h2>
                <p>We have received your payment.</p>
                <p><strong>Order ID:</strong> {orderId}</p>
                <p><strong>Amount Paid:</strong> {amount:C}</p>
                <p>Your order will be processed shortly.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment received email to {Email}", toEmail);
        }
    }

    public async Task SendListingCreatedAsync(string toEmail, string listingTitle)
    {
        try
        {
            var subject = $"Listing Created - {listingTitle}";
            var body = $@"
                <h2>Your listing has been created!</h2>
                <p>Your listing <strong>{listingTitle}</strong> is now live on the marketplace.</p>
                <p>You can manage your listing from your account dashboard.</p>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending listing created email to {Email}", toEmail);
        }
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";
        var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
        {
            _logger.LogWarning("Email configuration is missing. Email will not be sent.");
            return;
        }

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPassword)
        };

        using var message = new MailMessage(fromEmail, toEmail, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent successfully to {Email}", toEmail);
    }
}
