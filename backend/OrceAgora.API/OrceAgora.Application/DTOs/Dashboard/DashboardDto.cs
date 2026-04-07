namespace OrceAgora.Application.DTOs.Dashboard;

public class DashboardDto
{
    public decimal TotalThisMonth { get; set; }
    public decimal TotalLastMonth { get; set; }
    public decimal PercentChange { get; set; }
    public int SentThisMonth { get; set; }
    public int ApprovedThisMonth { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TotalBudgeted { get; set; }
    public decimal TotalApproved { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = [];
    public List<TopClientDto> TopClients { get; set; } = [];
    public List<TopServiceDto> TopServices { get; set; } = [];
    public List<PendingPaymentDto> PendingPayments { get; set; } = [];
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class TopClientDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class TopServiceDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Total { get; set; }
}

public class PendingPaymentDto
{
    public Guid BudgetId { get; set; }
    public int BudgetNumber { get; set; }
    public string? ClientName { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? DueDate { get; set; }
    public bool IsOverdue { get; set; }
}