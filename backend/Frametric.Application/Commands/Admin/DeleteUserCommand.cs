using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        // Check if performing user has permission to delete users
        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanDeleteUsers))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to delete users.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (targetUser == null) return false;

        // Protection rules:
        // 1. Cannot delete a SuperAdmin via API
        if (targetUser.Role == UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("SuperAdmin accounts cannot be deleted via the API.");
        }

        // 2. Standard Admins cannot delete other Admins (only SuperAdmin can delete Admins)
        if (targetUser.Role == UserRole.Admin && actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException("Standard Admins cannot delete other Admins.");
        }

        _context.Users.Remove(targetUser);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
