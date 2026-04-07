using System.ComponentModel.DataAnnotations;

namespace OrceAgora.Application.DTOs.Clients;

public record CreateClientDto(
    [Required] string Name,
    string? Phone,
    string? Email,
    string? Address
);

public record UpdateClientDto(
    [Required] string Name,
    string? Phone,
    string? Email,
    string? Address
);

public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
}