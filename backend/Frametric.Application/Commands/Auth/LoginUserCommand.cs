using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Auth;

public record LoginUserCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public LoginUserCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator, IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ArgumentException("Invalid email or password.");
        }

        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshTokenStr = _tokenGenerator.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Sliding expiry of 7 days

        var refreshToken = new RefreshToken(Guid.NewGuid(), user.Id, refreshTokenStr, refreshTokenExpiry);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(accessToken, refreshTokenStr, refreshTokenExpiry);
    }
}
