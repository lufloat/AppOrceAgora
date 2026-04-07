using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class LoginAttemptRepository(AppDbContext db) : ILoginAttemptRepository
{
    public Task<int> CountRecentFailuresAsync(string email, string ip, TimeSpan window)
    {
        var since = DateTime.UtcNow - window;
        return db.LoginAttempts
            .CountAsync(a => !a.Success && a.AttemptedAt >= since
                && (a.Email == email || a.Ip == ip));
    }

    public async Task AddAsync(LoginAttempt attempt) =>
        await db.LoginAttempts.AddAsync(attempt);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}