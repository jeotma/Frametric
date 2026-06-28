using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record UpdateDirectorCommand(Guid DirectorId, string Name) : IRequest<bool>;

public class UpdateDirectorCommandHandler : IRequestHandler<UpdateDirectorCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDirectorCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateDirectorCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanManageCatalog))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to manage the catalog.");
        }

        var director = await _context.Directors.FirstOrDefaultAsync(d => d.Id == request.DirectorId, cancellationToken);
        if (director == null) return false;

        director.UpdateName(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
