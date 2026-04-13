namespace OrceAgora.Domain.Entities;

public class AgendaEvent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? BudgetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ReminderAt { get; set; }
    public bool Done { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Budget? Budget { get; set; }
}


// verificar se essa agenda está de acordo com o que há na página.