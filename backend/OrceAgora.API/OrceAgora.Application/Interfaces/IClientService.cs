using OrceAgora.Application.DTOs.Clients;

namespace OrceAgora.Application.Interfaces;

public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync(Guid userId, string? search);
    Task<ClientDto?> GetByIdAsync(Guid id, Guid userId);
    Task<ClientDto> CreateAsync(Guid userId, CreateClientDto dto);
    Task<ClientDto?> UpdateAsync(Guid id, Guid userId, UpdateClientDto dto);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}