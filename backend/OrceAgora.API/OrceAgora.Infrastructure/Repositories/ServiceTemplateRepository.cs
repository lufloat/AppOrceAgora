using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Repositories;

public class ServiceTemplateRepository(AppDbContext db) : IServiceTemplateRepository
{
    public Task<ServiceTemplate?> GetByIdAsync(Guid id, Guid userId) =>
        db.ServiceTemplates
          .Include(t => t.Category)
          .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<List<ServiceTemplate>> GetByUserAsync(Guid userId, Guid? categoryId)
    {
        var query = db.ServiceTemplates
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId);

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task AddAsync(ServiceTemplate t) => await db.ServiceTemplates.AddAsync(t);
    public Task UpdateAsync(ServiceTemplate t) { db.ServiceTemplates.Update(t); return Task.CompletedTask; }
    public Task DeleteAsync(ServiceTemplate t) { db.ServiceTemplates.Remove(t); return Task.CompletedTask; }

    public async Task<ServiceTemplate> DuplicateAsync(ServiceTemplate original)
    {
        var copy = new ServiceTemplate
        {
            UserId = original.UserId,
            CategoryId = original.CategoryId,
            Name = original.Name + " (cópia)",
            DefaultPrice = original.DefaultPrice,
            Description = original.Description
        };
        await db.ServiceTemplates.AddAsync(copy);
        await db.SaveChangesAsync();
        return copy;
    }

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}