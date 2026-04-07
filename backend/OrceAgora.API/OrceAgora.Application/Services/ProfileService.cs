using OrceAgora.Application.DTOs.Auth;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class ProfileService(IUserRepository userRepo) : IProfileService
{
    public async Task<UserDto?> GetAsync(Guid userId)
    {
        var user = await userRepo.GetByIdAsync(userId);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return null;

        user.Name = dto.Name;
        user.CompanyName = dto.CompanyName;
        user.Phone = dto.Phone;
        user.Address = dto.Address;
        if (!string.IsNullOrWhiteSpace(dto.BrandColor))
            user.BrandColor = dto.BrandColor;

        await userRepo.UpdateAsync(user);
        await userRepo.SaveChangesAsync();
        return Map(user);
    }

    public async Task<UserDto?> UpdateLogoAsync(Guid userId, string logoBase64)
    {
        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return null;

        // Salva base64 direto — em produção usar Supabase Storage
        user.LogoUrl = logoBase64;
        await userRepo.UpdateAsync(user);
        await userRepo.SaveChangesAsync();
        return Map(user);
    }

    private static UserDto Map(OrceAgora.Domain.Entities.User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Plan = u.Plan.ToString().ToLower(),
        Phone = u.Phone,
        CompanyName = u.CompanyName,
        LogoUrl = u.LogoUrl,
        BrandColor = u.BrandColor
    };
}