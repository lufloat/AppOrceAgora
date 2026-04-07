using OrceAgora.Application.DTOs.Templates;

namespace OrceAgora.Application.Interfaces;

public interface IServiceTemplateService
{
    Task<List<TemplateDto>> GetAllAsync(Guid userId, Guid? categoryId);
    Task<TemplateDto?> GetByIdAsync(Guid id, Guid userId);
    Task<TemplateDto> CreateAsync(Guid userId, CreateTemplateDto dto);
    Task<TemplateDto?> UpdateAsync(Guid id, Guid userId, UpdateTemplateDto dto);
    Task<TemplateDto?> DuplicateAsync(Guid id, Guid userId);
    Task<bool> DeleteAsync(Guid id, Guid userId);
}