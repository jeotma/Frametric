using Frametric.Application.Interfaces;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.Imports;

public record MarkImportsCompletedCommand() : IRequest<int>;

public class MarkImportsCompletedCommandHandler : IRequestHandler<MarkImportsCompletedCommand, int>
{
    private readonly IApplicationDbContext _context;

    public MarkImportsCompletedCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(MarkImportsCompletedCommand request, CancellationToken cancellationToken)
    {
        // Check if there are any pending movies in the entire system
        var hasPendingMovies = await _context.Movies
            .AnyAsync(m => m.EnrichmentStatus == EnrichmentStatus.Pending, cancellationToken);

        if (hasPendingMovies)
        {
            return 0; // Can't complete imports yet
        }

        // If no pending movies exist, all "Enriching" imports are fully enriched
        var enrichingImports = await _context.ImportHistories
            .Where(ih => ih.Status == "Enriching")
            .ToListAsync(cancellationToken);

        foreach (var import in enrichingImports)
        {
            import.UpdateStatus("Completed");
        }

        if (enrichingImports.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return enrichingImports.Count;
    }
}
