namespace OrceAgora.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Budget> Budgets { get; set; } = [];
}