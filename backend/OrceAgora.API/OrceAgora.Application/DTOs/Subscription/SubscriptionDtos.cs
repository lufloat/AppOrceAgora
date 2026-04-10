namespace OrceAgora.Application.DTOs.Subscription;

public class SubscriptionStatusDto
{
    public string Plan { get; set; } = "basic";
    public string Status { get; set; } = "active";
    public int BudgetsThisMonth { get; set; }
    public int BudgetLimit { get; set; }
    public bool CanCreateBudget { get; set; }
    public int RemainingBudgets { get; set; }
    public DateOnly? CurrentPeriodEnd { get; set; }
}

public record UpgradeDto(string? CpfCnpj);