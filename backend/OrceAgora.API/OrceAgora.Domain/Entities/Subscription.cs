namespace OrceAgora.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? AsaasCustomerId { get; set; }
    public string? AsaasSubscriptionId { get; set; }
    public string Status { get; set; } = "active";
    public bool CancelAtPeriodEnd { get; set; } = false;

    public string Plan { get; set; } = "basic";
    public DateOnly? CurrentPeriodStart { get; set; }
    public DateOnly? CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}