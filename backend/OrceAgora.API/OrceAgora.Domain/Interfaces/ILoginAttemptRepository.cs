using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface ILoginAttemptRepository
{
    Task<int> CountRecentFailuresAsync(string email, string ip, TimeSpan window);
    Task AddAsync(LoginAttempt attempt);
    Task SaveChangesAsync();
}