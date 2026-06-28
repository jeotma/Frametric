using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record UpdateActorCommand(Guid ActorId, string Name) : IRequest<bool>;

public class UpdateActorCommandHandler : IRequestHandler<UpdateActorCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateActorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateActorCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanManageCatalog))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to manage the catalog.");
        }

        var dbActor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == request.ActorId, cancellationToken);
        if (dbActor == null) return false;

        dbActor.UpdateName(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
