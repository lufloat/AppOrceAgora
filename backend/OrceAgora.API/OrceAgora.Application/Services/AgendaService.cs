using OrceAgora.Application.DTOs.Agenda;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class AgendaService(IAgendaRepository repo) : IAgendaService
{
    public async Task<List<AgendaEventDto>> GetAllAsync(Guid userId, bool? done)
    {
        var events = await repo.GetByUserAsync(userId, done);
        return events.Select(Map).ToList();
    }

    public async Task<AgendaEventDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var evt = await repo.GetByIdAsync(id, userId);
        return evt is null ? null : Map(evt);
    }

    public async Task<AgendaEventDto> CreateAsync(Guid userId, CreateAgendaEventDto dto)
    {
        var evt = new AgendaEvent
        {
            UserId = userId,
            Title = dto.Title,
            Notes = dto.Notes,
            ReminderAt = dto.ReminderAt,
            BudgetId = dto.BudgetId
        };
        await repo.AddAsync(evt);
        await repo.SaveChangesAsync();
        return Map(evt);
    }

    public async Task<AgendaEventDto?> UpdateAsync(Guid id, Guid userId, UpdateAgendaEventDto dto)
    {
        var evt = await repo.GetByIdAsync(id, userId);
        if (evt is null) return null;

        evt.Title = dto.Title;
        evt.Notes = dto.Notes;
        evt.ReminderAt = dto.ReminderAt;
        evt.Done = dto.Done;

        await repo.UpdateAsync(evt);
        await repo.SaveChangesAsync();
        return Map(evt);
    }

    public async Task<AgendaEventDto?> ToggleDoneAsync(Guid id, Guid userId)
    {
        var evt = await repo.GetByIdAsync(id, userId);
        if (evt is null) return null;

        evt.Done = !evt.Done;
        await repo.UpdateAsync(evt);
        await repo.SaveChangesAsync();
        return Map(evt);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var evt = await repo.GetByIdAsync(id, userId);
        if (evt is null) return false;
        await repo.DeleteAsync(evt);
        await repo.SaveChangesAsync();
        return true;
    }

    private static AgendaEventDto Map(AgendaEvent e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Notes = e.Notes,
        ReminderAt = e.ReminderAt,
        Done = e.Done,
        BudgetId = e.BudgetId,
        BudgetNumber = e.Budget?.Number,
        ClientName = e.Budget?.Client?.Name,
        IsOverdue = e.ReminderAt.HasValue &&
                    e.ReminderAt.Value < DateTime.UtcNow &&
                    !e.Done,
        CreatedAt = e.CreatedAt
    };
}