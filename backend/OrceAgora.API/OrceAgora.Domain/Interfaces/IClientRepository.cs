using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, Guid userId);
    Task<List<Client>> GetByUserAsync(Guid userId, string? search);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(Client client);
    Task SaveChangesAsync();
}