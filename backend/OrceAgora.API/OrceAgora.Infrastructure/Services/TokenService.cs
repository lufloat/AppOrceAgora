using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;

namespace OrceAgora.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? _config["Jwt__Key"]!));

        var issuer = _config["Jwt:Issuer"] ?? _config["Jwt__Issuer"] ?? "orceAgora";
        var audience = _config["Jwt:Audience"] ?? _config["Jwt__Audience"] ?? "orceAgora";
        var days = int.Parse(_config["Jwt:ExpiresInDays"]
                       ?? _config["Jwt__ExpiresInDays"] ?? "30");

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("plan", user.Plan.ToString().ToLower()),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(days),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}