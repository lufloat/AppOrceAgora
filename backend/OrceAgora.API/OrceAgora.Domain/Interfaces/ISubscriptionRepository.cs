using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByUserIdAsync(Guid userId);
    Task<Subscription?> GetByAsaasSubscriptionIdAsync(string subscriptionId);
    Task AddAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task SaveChangesAsync();
}