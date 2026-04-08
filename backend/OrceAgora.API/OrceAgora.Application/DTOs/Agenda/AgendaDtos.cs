using System.ComponentModel.DataAnnotations;

namespace OrceAgora.Application.DTOs.Agenda;

public record CreateAgendaEventDto(
    [Required] string Title,
    string? Notes,
    DateTime? ReminderAt,
    Guid? BudgetId
);

public record UpdateAgendaEventDto(
    [Required] string Title,
    string? Notes,
    DateTime? ReminderAt,
    bool Done
);

public class AgendaEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? ReminderAt { get; set; }
    public bool Done { get; set; }
    public Guid? BudgetId { get; set; }
    public int? BudgetNumber { get; set; }
    public string? ClientName { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
}