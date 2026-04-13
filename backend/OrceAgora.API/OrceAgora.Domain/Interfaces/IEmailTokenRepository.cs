using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IEmailTokenRepository
{
    Task<EmailToken?> GetByTokenAsync(Guid token, string type);
    Task AddAsync(EmailToken emailToken);
    Task UpdateAsync(EmailToken emailToken);
    Task SaveChangesAsync();
}