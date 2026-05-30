using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Frametric.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Frametric.Application.Commands.EnrichMovies;

public class EnrichPendingMoviesCommandHandler : IRequestHandler<EnrichPendingMoviesCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly ITmdbService _tmdbService;

    public EnrichPendingMoviesCommandHandler(IApplicationDbContext context, ITmdbService tmdbService)
    {
        _context = context;
        _tmdbService = tmdbService;
    }

    public async Task<int> Handle(EnrichPendingMoviesCommand request, CancellationToken cancellationToken)
    {
        var pendingMovies = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Directors)
            .Include(m => m.Actors)
            .Where(m => m.EnrichmentStatus == EnrichmentStatus.Pending)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (!pendingMovies.Any()) return 0;

        int enrichedCount = 0;

        foreach (var movie in pendingMovies)
        {
            var tmdbData = await _tmdbService.SearchAndGetMovieDetailsAsync(movie.Title, movie.ReleaseYear, cancellationToken);

            if (tmdbData == null)
            {
                movie.MarkEnrichmentFailed();
                continue;
            }

            if (tmdbData.IsTvShow)
            {
                // It's a TV show/miniseries: move it out of Movies into TvShows
                var tvShow = new TvShow(Guid.NewGuid(), movie.Title, movie.ReleaseYear, tmdbData.TmdbId, tmdbData.PosterUrl);
                _context.TvShows.Add(tvShow);
                _context.Movies.Remove(movie);
                enrichedCount++;
                continue;
            }

            // Standard movie enrichment
            var genres = new List<Genre>();
            foreach (var gDto in tmdbData.Genres)
            {
                var genre = _context.Genres.Local.FirstOrDefault(g => g.TmdbId == gDto.Id)
                            ?? await _context.Genres.FirstOrDefaultAsync(g => g.TmdbId == gDto.Id, cancellationToken);
                if (genre == null)
                {
                    genre = new Genre(Guid.NewGuid(), gDto.Id, gDto.Name);
                    _context.Genres.Add(genre);
                }
                genres.Add(genre);
            }

            var directors = new List<Director>();
            foreach (var dDto in tmdbData.Directors)
            {
                var director = _context.Directors.Local.FirstOrDefault(d => d.TmdbId == dDto.Id)
                               ?? await _context.Directors.FirstOrDefaultAsync(d => d.TmdbId == dDto.Id, cancellationToken);
                if (director == null)
                {
                    director = new Director(Guid.NewGuid(), dDto.Id, dDto.Name);
                    _context.Directors.Add(director);
                }
                directors.Add(director);
            }

            var actors = new List<Actor>();
            foreach (var aDto in tmdbData.Actors)
            {
                var actor = _context.Actors.Local.FirstOrDefault(a => a.TmdbId == aDto.Id)
                            ?? await _context.Actors.FirstOrDefaultAsync(a => a.TmdbId == aDto.Id, cancellationToken);
                if (actor == null)
                {
                    actor = new Actor(Guid.NewGuid(), aDto.Id, aDto.Name);
                    _context.Actors.Add(actor);
                }
                actors.Add(actor);
            }

            movie.EnrichMetadata(tmdbData.RuntimeMinutes ?? 0, tmdbData.PosterUrl ?? string.Empty, genres, directors, actors);
            enrichedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return enrichedCount;
    }
}
