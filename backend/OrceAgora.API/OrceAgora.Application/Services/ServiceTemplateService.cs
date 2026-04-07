using OrceAgora.Application.DTOs.Templates;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class ServiceTemplateService(IServiceTemplateRepository repo) : IServiceTemplateService
{
    public async Task<List<TemplateDto>> GetAllAsync(Guid userId, Guid? categoryId)
    {
        var items = await repo.GetByUserAsync(userId, categoryId);
        return items.Select(Map).ToList();
    }

    public async Task<TemplateDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var t = await repo.GetByIdAsync(id, userId);
        return t is null ? null : Map(t);
    }

    public async Task<TemplateDto> CreateAsync(Guid userId, CreateTemplateDto dto)
    {
        var template = new ServiceTemplate
        {
            UserId = userId,
            Name = dto.Name,
            DefaultPrice = dto.DefaultPrice,
            Description = dto.Description,
            CategoryId = dto.CategoryId
        };
        await repo.AddAsync(template);
        await repo.SaveChangesAsync();
        return Map(template);
    }

    public async Task<TemplateDto?> UpdateAsync(Guid id, Guid userId, UpdateTemplateDto dto)
    {
        var template = await repo.GetByIdAsync(id, userId);
        if (template is null) return null;

        template.Name = dto.Name;
        template.DefaultPrice = dto.DefaultPrice;
        template.Description = dto.Description;
        template.CategoryId = dto.CategoryId;

        await repo.UpdateAsync(template);
        await repo.SaveChangesAsync();
        return Map(template);
    }

    public async Task<TemplateDto?> DuplicateAsync(Guid id, Guid userId)
    {
        var template = await repo.GetByIdAsync(id, userId);
        if (template is null) return null;
        var copy = await repo.DuplicateAsync(template);
        return Map(copy);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var template = await repo.GetByIdAsync(id, userId);
        if (template is null) return false;
        await repo.DeleteAsync(template);
        await repo.SaveChangesAsync();
        return true;
    }

    private static TemplateDto Map(ServiceTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        DefaultPrice = t.DefaultPrice,
        Description = t.Description,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name,
        CreatedAt = t.CreatedAt
    };
}