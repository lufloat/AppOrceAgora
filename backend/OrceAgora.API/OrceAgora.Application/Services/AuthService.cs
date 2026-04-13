using Microsoft.Extensions.Configuration;
using OrceAgora.Application.DTOs.Auth;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class AuthService(
    IUserRepository userRepo,
    ITokenService tokenService,
    ILoginAttemptRepository attemptRepo,
    IEmailTokenRepository emailTokenRepo,
    IEmailService emailService,
    IConfiguration config) : IAuthService
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan LockWindow = TimeSpan.FromMinutes(15);

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
    {
        if (await userRepo.ExistsByEmailAsync(dto.Email))
            return null;

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CompanyName = dto.CompanyName,
            Phone = dto.Phone,
            EmailConfirmed = false
        };

        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        var emailToken = new EmailToken
        {
            UserId = user.Id,
            Type = "confirm_email",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
        await emailTokenRepo.AddAsync(emailToken);
        await emailTokenRepo.SaveChangesAsync();

        var appUrl = Environment.GetEnvironmentVariable("Resend__AppUrl")
            ?? config["Resend:AppUrl"]
            ?? "http://localhost:5173";
        var confirmUrl = $"{appUrl}/confirmar-email?token={emailToken.Token}";

        await emailService.SendConfirmationEmailAsync(
            user.Email, user.Name, confirmUrl);

        return new AuthResponseDto
        {
            Token = tokenService.GenerateToken(user),
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Plan = user.Plan.ToString().ToLower(),
                Phone = user.Phone,
                CompanyName = user.CompanyName,
                LogoUrl = user.LogoUrl,
                EmailConfirmed = false
            }
        };
    }

    public async Task<(AuthResponseDto? Result, bool IsLocked, int RemainingAttempts)>
        LoginAsync(LoginDto dto, string ip)
    {
        var failures = await attemptRepo.CountRecentFailuresAsync(
            dto.Email.ToLower(), ip, LockWindow);

        if (failures >= MaxAttempts)
            return (null, true, 0);

        var user = await userRepo.GetByEmailAsync(dto.Email.ToLower());
        var success = user is not null &&
                      BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        await attemptRepo.AddAsync(new LoginAttempt
        {
            Email = dto.Email.ToLower(),
            Ip = ip,
            Success = success
        });
        await attemptRepo.SaveChangesAsync();

        if (!success)
            return (null, false, MaxAttempts - failures - 1);

        return (BuildResponse(user!), false, MaxAttempts);
    }

    public async Task<AuthResponseDto?> GoogleLoginAsync(string accessToken)
    {
        var info = await ValidateGoogleTokenAsync(accessToken);
        if (info is null) return null;

        var user = await userRepo.GetByEmailAsync(info.Value.Email.ToLower());

        if (user is null)
        {
            user = new User
            {
                Name = info.Value.Name,
                Email = info.Value.Email.ToLower(),
                PasswordHash = string.Empty,
                GoogleId = info.Value.Subject,
                EmailConfirmed = true
            };
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();
        }
        else if (user.GoogleId is null)
        {
            user.GoogleId = info.Value.Subject;
            user.EmailConfirmed = true;
            await userRepo.UpdateAsync(user);
            await userRepo.SaveChangesAsync();
        }

        return BuildResponse(user);
    }

    public async Task<bool> ConfirmEmailAsync(Guid token)
    {
        var emailToken = await emailTokenRepo
            .GetByTokenAsync(token, "confirm_email");

        if (emailToken is null || !emailToken.IsValid) return false;

        emailToken.User.EmailConfirmed = true;
        emailToken.UsedAt = DateTime.UtcNow;

        await userRepo.UpdateAsync(emailToken.User);
        await emailTokenRepo.UpdateAsync(emailToken);
        await emailTokenRepo.SaveChangesAsync();
        return true;
    }

    public async Task SendPasswordResetAsync(string email)
    {
        var user = await userRepo.GetByEmailAsync(email.ToLower());
        if (user is null) return;

        var emailToken = new EmailToken
        {
            UserId = user.Id,
            Type = "reset_password",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        await emailTokenRepo.AddAsync(emailToken);
        await emailTokenRepo.SaveChangesAsync();

        var appUrl = Environment.GetEnvironmentVariable("Resend__AppUrl")
            ?? config["Resend:AppUrl"]
            ?? "http://localhost:5173";
        var resetUrl = $"{appUrl}/redefinir-senha?token={emailToken.Token}";

        await emailService.SendPasswordResetEmailAsync(
            user.Email, user.Name, resetUrl);
    }

    public async Task<bool> ResetPasswordAsync(Guid token, string newPassword)
    {
        var emailToken = await emailTokenRepo
            .GetByTokenAsync(token, "reset_password");

        if (emailToken is null || !emailToken.IsValid) return false;

        emailToken.User.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(newPassword);
        emailToken.UsedAt = DateTime.UtcNow;

        await userRepo.UpdateAsync(emailToken.User);
        await emailTokenRepo.UpdateAsync(emailToken);
        await emailTokenRepo.SaveChangesAsync();
        return true;
    }

    private static async Task<(string Email, string Name, string Subject)?>
        ValidateGoogleTokenAsync(string accessToken)
    {
        try
        {
            using var http = new HttpClient();
            var response = await http.GetAsync(
                $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}");

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var email = root.GetProperty("email").GetString()!;
            var name = root.GetProperty("name").GetString()!;
            var sub = root.GetProperty("sub").GetString()!;

            return (email, name, sub);
        }
        catch
        {
            return null;
        }
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        Token = tokenService.GenerateToken(user),
        User = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Plan = user.Plan.ToString().ToLower(),
            Phone = user.Phone,
            CompanyName = user.CompanyName,
            LogoUrl = user.LogoUrl,
            EmailConfirmed = user.EmailConfirmed
        }
    };
}