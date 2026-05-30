using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Imports;

public record DeleteImportCommand(Guid UserId, Guid ImportId) : IRequest<bool>;

public class DeleteImportCommandHandler : IRequestHandler<DeleteImportCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteImportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteImportCommand request, CancellationToken cancellationToken)
    {
        var import = await _context.ImportHistories
            .FirstOrDefaultAsync(ih => ih.Id == request.ImportId && ih.UserId == request.UserId, cancellationToken);

        if (import == null)
        {
            throw new KeyNotFoundException("Import history log not found or does not belong to the user.");
        }

        // Delete all dependent entities associated with this ImportHistoryId in direct database commands
        await _context.DiaryEntries
            .Where(de => de.ImportHistoryId == request.ImportId && de.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.MovieRatings
            .Where(mr => mr.ImportHistoryId == request.ImportId && mr.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.WatchlistItems
            .Where(wi => wi.ImportHistoryId == request.ImportId && wi.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.MovieLikes
            .Where(ml => ml.ImportHistoryId == request.ImportId && ml.UserId == request.UserId)
            .ExecuteDeleteAsync(cancellationToken);

        // Finally delete the import history record itself
        _context.ImportHistories.Remove(import);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
