namespace OrceAgora.Domain.Entities;

public class EmailToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid Token { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public bool IsValid => UsedAt is null && ExpiresAt > DateTime.UtcNow;
}