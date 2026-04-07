using OrceAgora.Application.DTOs.Budgets;

namespace OrceAgora.Application.Interfaces;

public interface IBudgetService
{
    Task<PaginatedBudgetsDto> GetAllAsync(Guid userId, string? status, int page, int pageSize);
    Task<BudgetDto?> GetByIdAsync(Guid id, Guid userId);
    Task<BudgetDto?> GetByApprovalTokenAsync(Guid token);
    Task<BudgetDto> CreateAsync(Guid userId, CreateBudgetDto dto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
    Task<BudgetDto?> ProcessApprovalAsync(Guid token, ApproveRejectDto dto);
    Task<(BudgetDto Budget, byte[] Pdf)?> GeneratePdfAsync(Guid id, Guid userId);
}