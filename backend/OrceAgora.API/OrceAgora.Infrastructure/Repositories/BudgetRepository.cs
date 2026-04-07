using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Enums;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class BudgetRepository(AppDbContext db) : IBudgetRepository
{
    public Task<Budget?> GetByIdAsync(Guid id, Guid userId) =>
        db.Budgets
          .Include(b => b.Client)
          .Include(b => b.Items)
          .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

    public Task<Budget?> GetByApprovalTokenAsync(Guid token) =>
        db.Budgets
          .Include(b => b.Client)
          .Include(b => b.Items)
          .FirstOrDefaultAsync(b => b.ApprovalToken == token);

    public async Task<(List<Budget> Items, int Total)> GetByUserAsync(
        Guid userId, string? status, int page, int pageSize)
    {
        var query = db.Budgets
            .Include(b => b.Client)
            .Where(b => b.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<BudgetStatus>(status, true, out var parsedStatus))
            query = query.Where(b => b.Status == parsedStatus);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Budget budget) => await db.Budgets.AddAsync(budget);
    public Task UpdateAsync(Budget b) { db.Budgets.Update(b); return Task.CompletedTask; }
    public Task DeleteAsync(Budget b) { db.Budgets.Remove(b); return Task.CompletedTask; }
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}