using OrceAgora.Application.DTOs.Clients;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class ClientService(IClientRepository repo) : IClientService
{
    public async Task<List<ClientDto>> GetAllAsync(Guid userId, string? search)
    {
        var clients = await repo.GetByUserAsync(userId, search);
        return clients.Select(Map).ToList();
    }

    public async Task<ClientDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var client = await repo.GetByIdAsync(id, userId);
        return client is null ? null : Map(client);
    }

    public async Task<ClientDto> CreateAsync(Guid userId, CreateClientDto dto)
    {
        var client = new Client
        {
            UserId = userId,
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address
        };
        await repo.AddAsync(client);
        await repo.SaveChangesAsync();
        return Map(client);
    }

    public async Task<ClientDto?> UpdateAsync(Guid id, Guid userId, UpdateClientDto dto)
    {
        var client = await repo.GetByIdAsync(id, userId);
        if (client is null) return null;

        client.Name = dto.Name;
        client.Phone = dto.Phone;
        client.Email = dto.Email;
        client.Address = dto.Address;

        await repo.UpdateAsync(client);
        await repo.SaveChangesAsync();
        return Map(client);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var client = await repo.GetByIdAsync(id, userId);
        if (client is null) return false;
        await repo.DeleteAsync(client);
        await repo.SaveChangesAsync();
        return true;
    }

    private static ClientDto Map(Client c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        CreatedAt = c.CreatedAt
    };
}