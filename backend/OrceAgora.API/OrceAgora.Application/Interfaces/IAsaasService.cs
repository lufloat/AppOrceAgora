namespace OrceAgora.Application.Interfaces;

public class AsaasSubscriptionResult
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public string? PixCode { get; set; }
    public string? PixQrCodeUrl { get; set; }
}

public interface IAsaasService
{
    Task<string> CreateCustomerAsync(string name, string email, string? cpfCnpj);
    Task<AsaasSubscriptionResult> CreateSubscriptionAsync(string customerId, string planName);
    Task CancelSubscriptionAsync(string subscriptionId);
}