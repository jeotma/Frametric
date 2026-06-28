using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record UpdateUserPermissionsCommand(
    Guid UserId, 
    bool CanManageCatalog, 
    bool CanAddUsers, 
    bool CanDeleteUsers, 
    bool CanPromoteToAdmin) : IRequest<bool>;

public class UpdateUserPermissionsCommandHandler : IRequestHandler<UpdateUserPermissionsCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserPermissionsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        // Only SuperAdmin can modify permission delegation
        if (actor.Role != UserRole.SuperAdmin)
        {
            throw new UnauthorizedAccessException("Only SuperAdmin can delegate privileges.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (targetUser == null) return false;

        // Permissions only apply to Admins
        if (targetUser.Role != UserRole.Admin)
        {
            throw new InvalidOperationException("Permissions can only be configured for Admin users.");
        }

        targetUser.UpdatePermissions(
            request.CanManageCatalog,
            request.CanAddUsers,
            request.CanDeleteUsers,
            request.CanPromoteToAdmin
        );

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
