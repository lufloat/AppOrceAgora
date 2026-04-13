using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class EmailTokenRepository(AppDbContext db) : IEmailTokenRepository
{
    public Task<EmailToken?> GetByTokenAsync(Guid token, string type) =>
        db.EmailTokens
          .Include(t => t.User)
          .FirstOrDefaultAsync(t => t.Token == token && t.Type == type);

    public async Task AddAsync(EmailToken emailToken) =>
        await db.EmailTokens.AddAsync(emailToken);

    public Task UpdateAsync(EmailToken emailToken)
    {
        db.EmailTokens.Update(emailToken);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}