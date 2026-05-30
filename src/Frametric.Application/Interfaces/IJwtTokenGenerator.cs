using Frametric.Domain.Entities;

namespace Frametric.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
