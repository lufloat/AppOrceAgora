using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IServiceTemplateRepository
{
    Task<ServiceTemplate?> GetByIdAsync(Guid id, Guid userId);
    Task<List<ServiceTemplate>> GetByUserAsync(Guid userId, Guid? categoryId);
    Task AddAsync(ServiceTemplate template);
    Task UpdateAsync(ServiceTemplate template);
    Task DeleteAsync(ServiceTemplate template);
    Task<ServiceTemplate> DuplicateAsync(ServiceTemplate template);
    Task SaveChangesAsync();
}