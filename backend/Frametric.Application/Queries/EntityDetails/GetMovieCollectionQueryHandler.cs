using Frametric.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Queries.EntityDetails;

public class GetMovieCollectionQueryHandler : IRequestHandler<GetMovieCollectionQuery, MovieCollectionResult?>
{
    private readonly IApplicationDbContext _context;

    public GetMovieCollectionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MovieCollectionResult?> Handle(GetMovieCollectionQuery request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Where(m => m.Id == request.MovieId)
            .Select(m => new { m.TmdbCollectionId, m.TmdbCollectionName })
            .FirstOrDefaultAsync(cancellationToken);

        if (movie == null) return null;

        return new MovieCollectionResult(movie.TmdbCollectionId, movie.TmdbCollectionName);
    }
}
