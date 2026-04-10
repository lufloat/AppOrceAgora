namespace OrceAgora.Application.Interfaces;

public interface IAsaasService
{
    Task<string> CreateCustomerAsync(string name, string email, string? cpfCnpj);
    Task<string> CreateSubscriptionAsync(string customerId, string planName);
    Task CancelSubscriptionAsync(string subscriptionId);
}