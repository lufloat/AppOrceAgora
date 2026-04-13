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

    [HttpPost("google")]
    public async Task<IActionResult> Google(GoogleLoginDto dto)
    {
        var result = await authService.GoogleLoginAsync(dto.IdToken);
        if (result is null)
            return Unauthorized(new { message = "Token Google inválido." });
        return Ok(result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid token)
    {
        var success = await authService.ConfirmEmailAsync(token);
        if (!success)
            return BadRequest(new
            {
                message = "Link inválido ou expirado."
            });
        return Ok(new { message = "E-mail confirmado com sucesso!" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await authService.SendPasswordResetAsync(dto.Email);
        // Sempre retorna 200 — não revela se o e-mail existe
        return Ok(new
        {
            message = "Se este e-mail existir, você receberá as instruções."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var success = await authService
            .ResetPasswordAsync(dto.Token, dto.NewPassword);
        if (!success)
            return BadRequest(new
            {
                message = "Link inválido ou expirado."
            });
        return Ok(new { message = "Senha redefinida com sucesso!" });
    }
}