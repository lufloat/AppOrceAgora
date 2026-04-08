using OrceAgora.Application.DTOs.Agenda;

namespace OrceAgora.Application.Interfaces;

public interface IAgendaService
{
    Task<List<AgendaEventDto>> GetAllAsync(Guid userId, bool? done);
    Task<AgendaEventDto?> GetByIdAsync(Guid id, Guid userId);
    Task<AgendaEventDto> CreateAsync(Guid userId, CreateAgendaEventDto dto);
    Task<AgendaEventDto?> UpdateAsync(Guid id, Guid userId, UpdateAgendaEventDto dto);
    Task<AgendaEventDto?> ToggleDoneAsync(Guid id, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}