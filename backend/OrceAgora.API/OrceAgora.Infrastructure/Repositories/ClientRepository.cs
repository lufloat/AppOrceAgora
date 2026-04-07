using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class ClientRepository(AppDbContext db) : IClientRepository
{
    public Task<Client?> GetByIdAsync(Guid id, Guid userId) =>
        db.Clients.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    public async Task<List<Client>> GetByUserAsync(Guid userId, string? search)
    {
        var query = db.Clients.Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.ToLower().Contains(search.ToLower())
                                  || (c.Phone != null && c.Phone.Contains(search)));

        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task AddAsync(Client client) => await db.Clients.AddAsync(client);
    public Task UpdateAsync(Client client) { db.Clients.Update(client); return Task.CompletedTask; }
    public Task DeleteAsync(Client client) { db.Clients.Remove(client); return Task.CompletedTask; }
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}