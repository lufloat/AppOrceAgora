using OrceAgora.Application.DTOs.Dashboard;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Enums;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class DashboardService(IDashboardRepository repo) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startThis = new DateTime(now.Year, now.Month, 1);
        var startLast = startThis.AddMonths(-1);

        var budgets = await repo.GetBudgetsWithDetailsAsync(userId);
        var approved = budgets.Where(b => b.Status == BudgetStatus.Approved).ToList();

        var thisMonth = approved.Where(b => b.CreatedAt >= startThis).ToList();
        var lastMonth = approved.Where(b => b.CreatedAt >= startLast && b.CreatedAt < startThis).ToList();

        var totalThis = thisMonth.Sum(b => b.Total);
        var totalLast = lastMonth.Sum(b => b.Total);
        var percentChange = totalLast == 0 ? 0 : ((totalThis - totalLast) / totalLast) * 100;

        var sentThis = budgets.Count(b => b.CreatedAt >= startThis && b.Status != BudgetStatus.Draft);
        var approvedThis = thisMonth.Count;

        var monthly = Enumerable.Range(0, 6)
            .Select(i => startThis.AddMonths(-i))
            .Select(m => new MonthlyRevenueDto
            {
                Month = m.ToString("MMM/yy"),
                Total = approved
                    .Where(b => b.CreatedAt.Year == m.Year && b.CreatedAt.Month == m.Month)
                    .Sum(b => b.Total)
            })
            .Reverse()
            .ToList();

        var topClients = approved
            .Where(b => b.Client != null)
            .GroupBy(b => b.Client!.Name)
            .Select(g => new TopClientDto
            {
                Name = g.Key,
                Total = g.Sum(b => b.Total),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Total)
            .Take(5)
            .ToList();

        var topServices = approved
            .SelectMany(b => b.Items)
            .GroupBy(i => i.Name)
            .Select(g => new TopServiceDto
            {
                Name = g.Key,
                Count = g.Count(),
                Total = g.Sum(i => i.Qty * i.UnitPrice)
            })
            .OrderByDescending(s => s.Count)
            .Take(5)
            .ToList();

        var pending = budgets
            .SelectMany(b => b.Payments
                .Where(p => p.Status == PaymentStatus.Pending)
                .Select(p => new PendingPaymentDto
                {
                    BudgetId = b.Id,
                    BudgetNumber = b.Number,
                    ClientName = b.Client?.Name,
                    Amount = p.Amount,
                    DueDate = p.DueDate,
                    IsOverdue = p.DueDate.HasValue &&
                                p.DueDate.Value < DateOnly.FromDateTime(now)
                }))
            .OrderBy(p => p.DueDate)
            .Take(10)
            .ToList();

        return new DashboardDto
        {
            TotalThisMonth = totalThis,
            TotalLastMonth = totalLast,
            PercentChange = Math.Round(percentChange, 1),
            SentThisMonth = sentThis,
            ApprovedThisMonth = approvedThis,
            ConversionRate = sentThis == 0
                ? 0
                : Math.Round((decimal)approvedThis / sentThis * 100, 1),
            TotalBudgeted = budgets.Sum(b => b.Total),
            TotalApproved = approved.Sum(b => b.Total),
            MonthlyRevenue = monthly,
            TopClients = topClients,
            TopServices = topServices,
            PendingPayments = pending
        };
    }
}