using Microsoft.AspNetCore.Mvc;
using OrceAgora.Application.DTOs.Auth;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (result is null)
            return Conflict(new { message = "E-mail já cadastrado." });
        return Ok(result);
    }


    [HttpPost("google")]
    public async Task<IActionResult> Google(GoogleLoginDto dto)
    {
        var result = await authService.GoogleLoginAsync(dto.IdToken);
        if (result is null)
            return Unauthorized(new { message = "Token Google inválido." });
        return Ok(result);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var (result, isLocked, remaining) = await authService.LoginAsync(dto, ip);

        if (isLocked)
            return StatusCode(429, new
            {
                message = "Muitas tentativas. Tente novamente em 15 minutos."
            });

        if (result is null)
            return Unauthorized(new
            {
                message = remaining > 0
                    ? $"E-mail ou senha inválidos. {remaining} tentativa(s) restante(s)."
                    : "Conta temporariamente bloqueada."
            });

        return Ok(result);
    }
}