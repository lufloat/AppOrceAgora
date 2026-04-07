using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class DashboardRepository(AppDbContext db) : IDashboardRepository
{
    public Task<List<Budget>> GetBudgetsWithDetailsAsync(Guid userId) =>
        db.Budgets
          .Include(b => b.Client)
          .Include(b => b.Items)
          .Include(b => b.Payments)
          .Where(b => b.UserId == userId)
          .ToListAsync();
}