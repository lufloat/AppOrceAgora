using OrceAgora.Application.DTOs.Auth;

namespace OrceAgora.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    Task<(AuthResponseDto? Result, bool IsLocked, int RemainingAttempts)>
        LoginAsync(LoginDto dto, string ip);
    Task<AuthResponseDto?> GoogleLoginAsync(string accessToken);
}