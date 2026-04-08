using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class AgendaRepository(AppDbContext db) : IAgendaRepository
{
    public async Task<List<AgendaEvent>> GetByUserAsync(Guid userId, bool? done)
    {
        var query = db.AgendaEvents
            .Include(e => e.Budget)
                .ThenInclude(b => b != null ? b.Client : null)
            .Where(e => e.UserId == userId);

        if (done.HasValue)
            query = query.Where(e => e.Done == done.Value);

        return await query
            .OrderBy(e => e.Done)
            .ThenBy(e => e.ReminderAt == null)
            .ThenBy(e => e.ReminderAt)
            .ToListAsync();
    }

    public Task<AgendaEvent?> GetByIdAsync(Guid id, Guid userId) =>
        db.AgendaEvents
          .Include(e => e.Budget)
            .ThenInclude(b => b != null ? b.Client : null)
          .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

    public async Task AddAsync(AgendaEvent evt) =>
        await db.AgendaEvents.AddAsync(evt);

    public Task UpdateAsync(AgendaEvent evt)
    {
        db.AgendaEvents.Update(evt);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AgendaEvent evt)
    {
        db.AgendaEvents.Remove(evt);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}