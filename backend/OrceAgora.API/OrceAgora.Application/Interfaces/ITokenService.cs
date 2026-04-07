using OrceAgora.Domain.Entities;

namespace OrceAgora.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}