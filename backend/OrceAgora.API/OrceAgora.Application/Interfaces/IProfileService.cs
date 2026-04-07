using OrceAgora.Application.DTOs.Auth;

namespace OrceAgora.Application.Interfaces;

public interface IProfileService
{
    Task<UserDto?> GetAsync(Guid userId);
    Task<UserDto?> UpdateAsync(Guid userId, UpdateProfileDto dto);
    Task<UserDto?> UpdateLogoAsync(Guid userId, string logoBase64);
}