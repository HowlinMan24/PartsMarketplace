namespace Infrastructure.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string toEmail, string orderId, decimal totalAmount);
    Task SendPaymentReceivedAsync(string toEmail, string orderId, decimal amount);
    Task SendListingCreatedAsync(string toEmail, string listingTitle);
}
