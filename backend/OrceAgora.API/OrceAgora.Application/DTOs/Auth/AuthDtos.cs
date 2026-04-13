using System.ComponentModel.DataAnnotations;

namespace OrceAgora.Application.DTOs.Auth;

public record GoogleLoginDto([Required] string IdToken);

public record UpdateProfileDto(
    string Name,
    string? CompanyName,
    string? Phone,
    string? Address,
    string? BrandColor
);

public record ForgotPasswordDto([Required, EmailAddress] string Email);
public record ResetPasswordDto(
    [Required] Guid Token,
    [Required, MinLength(8)] string NewPassword
);

public record UpdateLogoDto(string LogoBase64);

public record RegisterDto(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    string? Phone,
    string? CompanyName
);

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? LogoUrl { get; set; }
    public string BrandColor { get; set; } = "#1A56DB";
    public bool EmailConfirmed { get; set; }
}