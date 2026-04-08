using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IAgendaRepository
{
    Task<List<AgendaEvent>> GetByUserAsync(Guid userId, bool? done);
    Task<AgendaEvent?> GetByIdAsync(Guid id, Guid userId);
    Task AddAsync(AgendaEvent evt);
    Task UpdateAsync(AgendaEvent evt);
    Task DeleteAsync(AgendaEvent evt);
    Task SaveChangesAsync();
}