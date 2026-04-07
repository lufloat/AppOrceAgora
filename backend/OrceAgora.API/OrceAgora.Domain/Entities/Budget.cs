using OrceAgora.Domain.Enums;

namespace OrceAgora.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ClientId { get; set; }
    public int Number { get; set; }
    public BudgetStatus Status { get; set; } = BudgetStatus.Draft;
    public decimal Subtotal { get; set; }
    public DiscountType DiscountType { get; set; } = DiscountType.Fixed;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Extras { get; set; }
    public string? ExtrasDescription { get; set; }
    public decimal Total { get; set; }
    public int ValidityDays { get; set; } = 7;
    public string? Notes { get; set; }
    public string? PaymentMethods { get; set; }
    public Guid ApprovalToken { get; set; } = Guid.NewGuid();
    public DateTime? ViewedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Client? Client { get; set; }
    public ICollection<BudgetItem> Items { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];

    public void RecalculateTotals()
    {
        Subtotal = Items.Sum(i => i.Total);
        DiscountAmount = DiscountType == DiscountType.Percent
            ? Subtotal * (DiscountValue / 100)
            : DiscountValue;
        Total = Subtotal - DiscountAmount + Extras;
    }
}