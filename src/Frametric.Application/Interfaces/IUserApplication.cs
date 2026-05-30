using Frametric.Application.DTOs;

namespace Frametric.Application.Interfaces;

public interface IUserApplication
{
    Task<Guid> RegisterAsync(string username, string email, string password, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
