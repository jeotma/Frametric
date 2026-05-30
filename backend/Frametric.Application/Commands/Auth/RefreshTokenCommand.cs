using Frametric.Application.DTOs;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public RefreshTokenCommandHandler(IApplicationDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var activeToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (activeToken == null || !activeToken.IsActive)
        {
            throw new ArgumentException("Invalid or expired refresh token.");
        }

        // Revoke the current token
        activeToken.Revoke();

        // Generate new tokens
        var user = activeToken.User;
        var newAccessToken = _tokenGenerator.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenGenerator.GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var newRefreshToken = new RefreshToken(Guid.NewGuid(), user.Id, newRefreshTokenStr, newRefreshTokenExpiry);
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(newAccessToken, newRefreshTokenStr, newRefreshTokenExpiry);
    }
}
