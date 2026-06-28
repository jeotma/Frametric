using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record DemoteUserCommand(Guid UserId) : IRequest<bool>;

public class DemoteUserCommandHandler : IRequestHandler<DemoteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DemoteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DemoteUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        // ONLY SuperAdmin can demote admins. Standard Admins cannot.
        if (actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can demote administrative users.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return false;

        // Cannot demote a SuperAdmin
        if (user.Role == UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("SuperAdmin cannot be demoted via API.");
        }

        user.DemoteToUser();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
