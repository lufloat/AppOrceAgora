using OrceAgora.Application.DTOs.Subscription;
using static OrceAgora.Application.DTOs.Subscription.SubscriptionStatusDto;

namespace OrceAgora.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionStatusDto> GetStatusAsync(Guid userId);
    Task<UpgradeResultDto> UpgradeToProAsync(Guid userId, UpgradeDto dto);
    Task CancelProAsync(Guid userId);
    Task HandleWebhookAsync(string payload);
}