using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.CustomLists;

public record RemoveMovieFromCustomListCommand(Guid UserId, Guid ListId, Guid MovieId) : IRequest<bool>;

public class RemoveMovieFromCustomListCommandHandler : IRequestHandler<RemoveMovieFromCustomListCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveMovieFromCustomListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveMovieFromCustomListCommand request, CancellationToken cancellationToken)
    {
        var list = await _context.CustomLists
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.ListId && c.UserId == request.UserId, cancellationToken);

        if (list == null) return false;

        var item = list.Items.FirstOrDefault(i => i.MovieId == request.MovieId);
        if (item != null)
        {
            list.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
