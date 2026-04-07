using OrceAgora.Domain.Entities;

namespace OrceAgora.Domain.Interfaces;

public interface IDashboardRepository
{
    Task<List<Budget>> GetBudgetsWithDetailsAsync(Guid userId);
}