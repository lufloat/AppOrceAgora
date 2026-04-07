using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, Guid userId);
    Task<Budget?> GetByApprovalTokenAsync(Guid token);
    Task<(List<Budget> Items, int Total)> GetByUserAsync(
        Guid userId, string? status, int page, int pageSize);
    Task AddAsync(Budget budget);
    Task UpdateAsync(Budget budget);
    Task DeleteAsync(Budget budget);
    Task SaveChangesAsync();
}