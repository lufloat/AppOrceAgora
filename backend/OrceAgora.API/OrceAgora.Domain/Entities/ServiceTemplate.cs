namespace OrceAgora.Domain.Entities;

public class ServiceTemplate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Category? Category { get; set; }
}