using OrceAgora.Application.DTOs.Subscription;

namespace OrceAgora.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionStatusDto> GetStatusAsync(Guid userId);
    Task<string> UpgradeToProAsync(Guid userId, UpgradeDto dto);
    Task CancelProAsync(Guid userId);
    Task HandleWebhookAsync(string payload);
}