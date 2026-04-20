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
    public bool CancelAtPeriodEnd { get; set; }
    public int? DaysRemainingAfterCancel { get; set; }
    public class AsaasSubscriptionResult
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? PixCode { get; set; }
        public string? PixQrCodeUrl { get; set; }
    }

    public class UpgradeResultDto
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
        public string? PixCode { get; set; }
        public string? PixQrCodeUrl { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

public record UpgradeDto(string? CpfCnpj);