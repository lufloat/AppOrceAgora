using System.ComponentModel.DataAnnotations;

namespace OrceAgora.Application.DTOs.Templates;

public record CreateTemplateDto(
    [Required] string Name,
    [Required, Range(0, double.MaxValue)] decimal DefaultPrice,
    string? Description,
    Guid? CategoryId
);

public record UpdateTemplateDto(
    [Required] string Name,
    [Required, Range(0, double.MaxValue)] decimal DefaultPrice,
    string? Description,
    Guid? CategoryId
);

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime CreatedAt { get; set; }
}