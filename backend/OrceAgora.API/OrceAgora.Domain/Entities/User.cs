using OrceAgora.Domain.Enums;

namespace OrceAgora.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string? GoogleId { get; set; }
    public bool EmailConfirmed { get; set; } = false;

    public string BrandColor { get; set; } = "#1A56DB";
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public PlanType Plan { get; set; } = PlanType.Basic;
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? CompanyName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<Client> Clients { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
    public ICollection<ServiceTemplate> ServiceTemplates { get; set; } = [];
    public ICollection<AgendaEvent> AgendaEvents { get; set; } = [];
}