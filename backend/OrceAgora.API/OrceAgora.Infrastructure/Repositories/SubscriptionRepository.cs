using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    public Task<Subscription?> GetByUserIdAsync(Guid userId) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);

    public Task<Subscription?> GetByAsaasSubscriptionIdAsync(string subscriptionId) =>
        db.Subscriptions.FirstOrDefaultAsync(s =>
            s.AsaasSubscriptionId == subscriptionId);

    public async Task AddAsync(Subscription subscription) =>
        await db.Subscriptions.AddAsync(subscription);

    public Task UpdateAsync(Subscription subscription)
    {
        db.Subscriptions.Update(subscription);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}