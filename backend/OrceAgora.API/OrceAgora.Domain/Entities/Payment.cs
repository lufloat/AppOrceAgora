using OrceAgora.Domain.Enums;

namespace OrceAgora.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateOnly? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public int InstallmentNumber { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Budget Budget { get; set; } = null!;
}