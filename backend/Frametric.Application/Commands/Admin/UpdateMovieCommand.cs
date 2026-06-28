using System;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Admin;

public record UpdateMovieCommand(Guid MovieId, string Title, string? Overview, int? ReleaseYear, int? RuntimeMinutes) : IRequest<bool>;

public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMovieCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null) throw new UnauthorizedAccessException("Not authenticated.");

        var actor = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
        if (actor == null) throw new UnauthorizedAccessException("Current user not found.");

        if (actor.Role != UserRole.SuperAdmin && !(actor.Role == UserRole.Admin && actor.CanManageCatalog))
        {
            throw new UnauthorizedAccessException("Insufficient privileges to manage the catalog.");
        }

        var movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == request.MovieId, cancellationToken);
        if (movie == null) return false;

        movie.UpdateMetadata(request.Title, request.Overview, request.ReleaseYear, request.RuntimeMinutes);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
